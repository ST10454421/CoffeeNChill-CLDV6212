## Docker Commands
Build: `docker build -t mananjalo0106/coffeenchill-functions:v1.0 .`
Push: `docker push mananjalo0106/coffeenchill-functions:v1.0`

Run Azurite: `docker run -d -p 10000:10000 -p 10001:10001 -p 10002:10002 --name azurite mcr.microsoft.com/azure-storage/azurite`

Run Functions container: `docker run -d -p 7071:80 -e AzureWebJobsStorage="DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://host.docker.internal:10000/devstoreaccount1;QueueEndpoint=http://host.docker.internal:10001/devstoreaccount1;TableEndpoint=http://host.docker.internal:10002/devstoreaccount1;" -e FileShareConnectionString="<your Azure Storage Account connection string>" --name coffeenchill-functions mananjalo0106/coffeenchill-functions:v1.0`

## Video
https://youtu.be/uEH-DP-cHK

## AI Disclosure
AI tools (Claude) were used for guidance on setup, debugging, and code structure throughout this project. All code was typed, tested, and understood by the author.

## Part 2: Queue Triggers and Docker Compose

### Part 2 Video
https://youtu.be/oqQRk8RktNE

### Changelog
- Added `QueueOrder` (POST /api/orders/queue): validates the order, serialises it to JSON and sends it to `order-processing-queue`
- Added `ProcessOrderQueue`: queue-triggered function that writes the order to the Orders table and moves the status Received, Preparing, Ready, Collected
- Added `GetOrders` (GET /api/orders) to check order status
- Added `docker-compose.yml` pulling `mananjalo0106/coffeenchill-functions:v2.0` from Docker Hub, with a custom bridge network and a volume for Azurite
- Added the Part 2 Postman collection in `/docs`

### Run with one command
From the repo root:

```
docker compose up
```

The Functions app is then available at http://localhost:7071

### Postman
Import `docs/coffeeNChill part 2.postman_collection.json` and run: Queue Order, Queue Order Invalid, Get Orders.

### Team contributions (Part 2)
- Member name (Mananjalo Mfeka): what they worked on( everthing was dine by me since i was left alone with no group)

## AI Disclosure
AI (Claude, Anthropic) was used in Part 2 for step-by-step guidance, explanations of Azure Storage Queues, queue-triggered functions and Docker Compose, and help debugging build and Azurite errors. I reviewed, tested and adapted the code, and I can explain how each part works.

## References
Docker Inc. (2026) *Docker Compose overview*. Available at: https://docs.docker.com/compose/ (Accessed: 12 October 2026).

Microsoft (2026) *Azure Queue storage trigger for Azure Functions*. Available at: https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-queue-trigger (Accessed: 12 October 2026).

Microsoft (2026) *Use the Azurite emulator for local Azure Storage development*. Available at: https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite (Accessed: 12 October 2026).

Postman (2026) *Write test scripts in Postman*. Available at: https://learning.postman.com/docs/tests-and-scripts/write-scripts/test-scripts/ (Accessed: 12 October 2026).

Anthropic (2026) *Claude* [Large language model]. Available at: https://claude.ai (Accessed: 12 October 2026).