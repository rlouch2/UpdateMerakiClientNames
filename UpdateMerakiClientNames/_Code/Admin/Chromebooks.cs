using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Discovery.v1.Data;
using Google.Apis.Discovery.v1;
using Google.Apis.Services;
using Meraki.Api;
using Meraki.Api.Data;
using RestSharp;
using UpdateMerakiClientNames.Objects;
using Serilog;
using System.Threading;

namespace UpdateMerakiClientNames._Code.Admin
{
    public class Chromebooks
    {
        // public ChromebookDeviceInfo deviceInfo { get; set; }

        public string? CustomerId { get; set; } = "";

        private DirectoryService googleDirectoryService { get; set; }
        private string MerakiOrgId { get; set; } = "";

        private string? MerakiApiKey { get; set; }

        public string MerakiClientName { get; set; }

        private List<Meraki.Api.Data.Network> MerakiNetworks { get; set; } = new List<Meraki.Api.Data.Network>();


        // public IRestResponse

        //public Chromebooks() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="merakiSettings"></param>
        public Chromebooks(UpdateMerakiClientNames.Objects.GoogleAuth settings, MerakiAuth merakiSettings)
        {

            this.CustomerId = settings.CustomerID;
            this.MerakiApiKey = merakiSettings.APIKey;

            googleDirectoryService = new DirectoryService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = CreateCredential(settings).Result,
                ApplicationName = settings.AppName,
                ApiKey = settings.APIKey
            });
        }

        /// <summary>
        /// Gets Chromebooks from Google Admin Console, provisions new clients inside of all Meraki networks
        /// </summary>
        /// <returns></returns>
        public async Task UpdateMerakiClientNameAsync()
        {
            ChromeosdevicesResource.ListRequest listRequest = googleDirectoryService.Chromeosdevices.List(CustomerId);
            ChromeOsDevices allDevices = listRequest.Execute();

            List<Objects.Meraki.client> merakiClients = new List<Objects.Meraki.client>();


            LoadDevicesForMeraki(allDevices, merakiClients);
            await MoveMerakiClient(merakiClients);
            merakiClients.Clear();

            while (allDevices.NextPageToken != null)
            {
                DateTime start = DateTime.Now;
                listRequest.PageToken = allDevices.NextPageToken;
                allDevices = listRequest.Execute();
                LoadDevicesForMeraki(allDevices, merakiClients);
                await MoveMerakiClient(merakiClients);
                merakiClients.Clear();
            }
        }

        /// <summary>
        /// Provision the clients in Mearki networks
        /// </summary>
        /// <param name="merakiClients"></param>
        /// <returns></returns>
        private async Task MoveMerakiClient(List<Objects.Meraki.client> merakiClients)
        {
            using var merakiClient = new MerakiClient(new MerakiClientOptions
            {
                ApiKey = MerakiApiKey
            });

            if (MerakiOrgId == null || MerakiOrgId == "")
                await GetMerakiOrgId(merakiClient);

            if (MerakiNetworks == null || MerakiNetworks.Count == 0)
                await GetMerakiNetworks(merakiClient);

            List<string> lstNetworks = MerakiNetworks.Select(n => n.Id).ToList();
            var regex = "(.{2})(.{2})(.{2})(.{2})(.{2})(.{2})";
            var replace = "$1:$2:$3:$4:$5:$6";

            List<Meraki.Api.Data.ClientProvision> clients = new List<Meraki.Api.Data.ClientProvision>();

            //Build the client objects outside of the network since in theory they could show up anywhere and we want to be able to recognize them
            foreach (Objects.Meraki.client chromebook in merakiClients)
            {


                Meraki.Api.Data.ClientProvision client = new Meraki.Api.Data.ClientProvision();


                client.Name = chromebook.name;

                //client.Mac = Regex.Replace(chromebook.mac, regex, replace);

                Log.Information(String.Format("Adding chromebook Device Name: {0}, MAC: {1}, DeviceID {2}", chromebook.name, client.Mac, chromebook.clientId));

                clients.Add(client);
            }


            foreach (Meraki.Api.Data.Network network in MerakiNetworks)
            {
                Log.Information(String.Format("Starting add of 100 clients for network {0}", network.Name));



                Meraki.Api.Data.ClientProvisionRequest clientProvisionRequest = new Meraki.Api.Data.ClientProvisionRequest();
                clientProvisionRequest.Clients = clients;
                clientProvisionRequest.DevicePolicy = Meraki.Api.Data.DevicePolicy.Normal;
                clientProvisionRequest.PoliciesBySecurityAppliance = null;
                clientProvisionRequest.PoliciesBySsid = null;

                var provisionRequest = await merakiClient.Networks.Clients.ProvisionNetworkClientsAsync(network.Id, clientProvisionRequest);

                Log.Information(String.Format("Finshed add of 100 clients for network {0}", network.Name));
            }
        }

        /// <summary>
        /// Get the Meraki organization ID for your Meraki install
        /// </summary>
        /// <param name="merakiClient"></param>
        /// <returns></returns>
        private async Task<string> GetMerakiOrgId(MerakiClient merakiClient)
        {
            CancellationToken cancellationToken = new CancellationToken();
            try
            {
                var organizations = await merakiClient
                    .Organizations
                    .GetOrganizationsAsync(cancellationToken)
                    .ConfigureAwait(false);


                MerakiOrgId = organizations[0].Id;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

            return MerakiOrgId;
        }

        /// <summary>
        /// Get all of the Meraki networks for your organization
        /// </summary>
        /// <param name="merakiClient"></param>
        /// <returns></returns>
        private async Task<List<Meraki.Api.Data.Network>> GetMerakiNetworks(MerakiClient merakiClient)
        {
            MerakiNetworks = await merakiClient
              .Organizations
              .Networks
              .GetOrganizationNetworksAsync(MerakiOrgId)
              .ConfigureAwait(false);

            List<ProductType> productTypes = new List<ProductType>();
            productTypes.Add(ProductType.Wireless);
            productTypes.Add(ProductType.Switch);

            List<Network> filteredNetworks = MerakiNetworks.Where(p => p.ProductTypes.Intersect(productTypes).Any()).ToList();
            MerakiNetworks = filteredNetworks;

            return MerakiNetworks;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="allDevices"></param>
        /// <param name="merakiClients"></param>
        private void LoadDevicesForMeraki(ChromeOsDevices allDevices, List<Objects.Meraki.client> merakiClients)
        {
            foreach (ChromeOsDevice device in allDevices.Chromeosdevices)
            {
                if (device.MacAddress != null)
                {
                    Objects.Meraki.client client = new Objects.Meraki.client();

                    client.mac = device.MacAddress;
                    client.name = GetProperty(device, MerakiClientName);

                    merakiClients.Add(client);
                }
            }
        }

        /// <summary>
        /// Create the Google credential login 
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        async Task<UserCredential> CreateCredential(UpdateMerakiClientNames.Objects.GoogleAuth settings)
        {
            UserCredential credential;
            using (var stream = new FileStream(settings.ClientSecretJsonFile, FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    new[] { DirectoryService.Scope.AdminDirectoryOrgunit,
                            DirectoryService.Scope.AdminDirectoryDeviceChromeos },
                    "user", CancellationToken.None);
            }

            return credential;
        }


        private string GetProperty(object target, string PropertyName)
        {
            int VariableCount = PropertyName.Count(v => v == '{');

            if (VariableCount == 0)
            {
                try
                {
                    var site = System.Runtime.CompilerServices.CallSite<Func<System.Runtime.CompilerServices.CallSite, object, object>>.Create(Microsoft.CSharp.RuntimeBinder.Binder.GetMember(0, PropertyName, target.GetType(), new[] { Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo.Create(0, null) }));
                    object val = site.Target(site, target);

                    if (val != null)
                        return val.ToString();
                    else
                        return "";
                }
                catch
                {
                    return "";
                }
            }
            else
            {
                StringBuilder sbProperty = new StringBuilder(PropertyName);
                int StartingStringLocation = PropertyName.IndexOf('{');
                int EndingStringLocation = PropertyName.IndexOf('}');
                int PropertyNameLength = EndingStringLocation - StartingStringLocation;
                for (int i = 0; i <= VariableCount - 1; i++)
                {
                    string subPropertyName = sbProperty.ToString().Substring(StartingStringLocation + 1, PropertyNameLength - 1);
                    string subPropertyValue = "";

                    var site = System.Runtime.CompilerServices.CallSite<Func<System.Runtime.CompilerServices.CallSite, object, object>>.Create(Microsoft.CSharp.RuntimeBinder.Binder.GetMember(0, subPropertyName, target.GetType(), new[] { Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo.Create(0, null) }));
                    object val = site.Target(site, target);

                    if (val != null)
                        subPropertyValue = val.ToString();
                    else
                        subPropertyValue = "";

                    sbProperty.Replace("{" + subPropertyName + "}", subPropertyValue);

                    StartingStringLocation = sbProperty.ToString().IndexOf('{');
                    EndingStringLocation = sbProperty.ToString().IndexOf('}');
                    PropertyNameLength = EndingStringLocation - StartingStringLocation;
                }

                return sbProperty.ToString();
            }
        }

    }
}