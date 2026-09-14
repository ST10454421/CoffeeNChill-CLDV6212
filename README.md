## Docker Commands
Build: `docker build -t mananjalo0106/coffeenchill-functions:v1.0 .`
Push: `docker push mananjalo0106/coffeenchill-functions:v1.0`

Run Azurite: `docker run -d -p 10000:10000 -p 10001:10001 -p 10002:10002 --name azurite mcr.microsoft.com/azure-storage/azurite`

Run Functions container: `docker run -d -p 7071:80 -e AzureWebJobsStorage="DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://host.docker.internal:10000/devstoreaccount1;QueueEndpoint=http://host.docker.internal:10001/devstoreaccount1;TableEndpoint=http://host.docker.internal:10002/devstoreaccount1;" -e FileShareConnectionString="<your Azure Storage Account connection string>" --name coffeenchill-functions mananjalo0106/coffeenchill-functions:v1.0`

## Video
https://youtu.be/uEH-DP-cHK

## AI Disclosure
AI tools (Claude) were used for guidance on setup, debugging, and code structure throughout this project. All code was typed, tested, and understood by the author.