# UpdateMerakiClientNames
Create/Update Meraki client names from Google Workspace devices

Rename the appsettings.json.template to appsettings.json

To run this project you will need to create an API key in Meraki

- Login into your Meraki dashboard and access your Meraki profile 
- Scroll down to the API Access section
- Click Generate new API Keya and copy this key - Once you acknowledge this you will not be able to copy it again and need to create a new one.
- This will be entered into the appsettings.json file where the MerakiAPI -> APIKey key/value field

Next log into [Google Cloud Console](https://console.cloud.google.com/apis/dashboard)
+ Create a new project called Meraki
+ Click on Enabled API's and Services
+ CLick on Enable APIS and Services
  + Add in Admin SDK Api

- Then navigate to the Credentials section
- Click the Create Credential - OAuth Client ID
  - Choose Desktop app
  - Name this credential MerakiOAuth
  - Click the Download JSON button to download the client secrets file
  - Rename this file client_secrets.json and copy this into the UpdateMerakiClientNames folder.

In the appsettings.json file you will also need to enter in your Google ClientID
This can be found here [Google ClientID](https://admin.google.com/ac/accountsettings)

You can control what the name of your Meraki Client name is based on the appsettings.json value stored in MerakiClientName
You can pull in any of the ChromeOS device properties listed out [here](https://developers.google.com/admin-sdk/directory/reference/rest/v1/chromeosdevices)
Example would get "GD-{SerialNumber}-{AnnotatedAssetId} - this will create a client name like GD-x123c23-1234
Or "SerialNumber" - this will create a client name like x123c23

At this point you should be ready to run this program. You will need to run from Visual Studio until I get an executable published.
