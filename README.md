# UpdateMerakiClientNames
Create/Update Meraki client names from Google Workspace devices

To run this project you will need to create an API key in Meraki

Login into your Meraki dashboard and access your Meraki profile 
Scroll down to the API Access section
Click Generate new API Keya and copy this key - Once you acknowledge this you will not be able to copy it again and need to create a new one.
This will be entered into the appsettings.json file where the MerakiAPI -> APIKey key/value field

Next log into https://console.cloud.google.com/apis/dashboard
Create a new project called Meraki
Then navigate to the Credentials section
Click the Create Credential - OAuth Client ID
Choose Desktop app
Name this credential MerakiOAuth
Click the Download JSON button to download the client secrets file
Rename this file client_secrets.json and copy this into the UpdateMerakiClientNames folder.

In the appsettings.json file you will also need to enter in your Google ClientID
This can be found here https://admin.google.com/ac/accountsettings

At this point you should be ready to run this program. You will need to run from Visual Studio until I get an executable published.
