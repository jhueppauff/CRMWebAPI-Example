# Not Tested yet
import http.client
import json

clientId = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
crmDomain = "crm.example.org"
crmApiPath = "/api/data/v8.2/"
redirectUri = "https://myawesomeapp.org"

adfsDomainName = "adfs.example.org"
adfsTokenPath = "/adfs/oauth2/token"

username = ""
password = ""

# Get AuthCode form ADFS
requestBodyADFS =  "resource=https://" + crmDomain + crmApiPath + "&client_id=" + clientId + "&grant_type=password&username=" + username + "&password=" + password + "&scope=openid&redirect_uri=" + redirectUri

connAdfs = http.client.HTTPSConnection(adfsDomainName)

payload = ""

adfsHeaders = {
    'content-type': "application/x-www-form-urlencoded",
    'cache-control': "no-cache",
    }


connAdfs.request("POST", adfsTokenPath , requestBodyADFS, adfsHeaders)

adfsResponse = connAdfs.getresponse()
data = adfsResponse.read()

print("ADFS Response:")
print(data.decode("utf-8"))

token = json.loads(data.decode("utf-8"))
authToken = "Bearer " + token["access_token"]

connCrm = http.client.HTTPSConnection(crmDomain)

crmHeaders = {
    "authorization" : authToken,
    'cache-control': "no-cache",
    'content-type': "application/json"
    }

connCrm.request("GET", crmApiPath + "WhoAmI()", None, crmHeaders)

crmResponse = connCrm.getresponse()
crmdata = crmResponse.read()
print("CRM Response")
print(crmdata.decode("utf-8"))
