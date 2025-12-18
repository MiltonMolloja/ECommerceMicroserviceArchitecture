@echo off
echo Starting RabbitMQ...
docker compose up -d rabbitmq
echo.
echo RabbitMQ started! Management UI: http://localhost:15672 (guest/guest)
echo.
echo Opening Visual Studio solution...
start ECommerce.sln
