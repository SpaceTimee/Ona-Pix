import clr
import sys
import requests
import json
clr.AddReference("Ona-Pix-Secret")
from OnaPixSecret import *

def upload(path):
    url = "https://sm.ms/api/v2/upload"
    files = {"smfile": open(path, "rb")}
    headers = {"Authorization": Secret.GetSmmsApiKey()}
    print(json.dumps(requests.post(url, files = files, headers = headers).json()))

if __name__ == "__main__":
    upload(sys.argv[1])
    #upload(r"D:\菜单\资源\ZIPANG.png")