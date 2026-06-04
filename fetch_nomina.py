import urllib3
import requests

# Disable SSL warning
urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)

try:
    url = "https://localhost:7097/Nomina"
    print("Fetching", url)
    res = requests.get(url, verify=False, timeout=10)
    print("Status code:", res.status_code)
    if res.status_code != 200:
        print("Response body:")
        print(res.text[:2000])
    else:
        print("Success! Response is 200 OK.")
except Exception as e:
    print("Error:", e)
