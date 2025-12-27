-- Create Logging schema and Logs table for PostgreSQL

CREATE SCHEMA IF NOT EXISTS "Logging";

CREATE TABLE IF NOT EXISTS "Logging"."Logs" (
    "LogId" SERIAL PRIMARY KEY,
    "Timestamp" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "LogLevel" VARCHAR(50) NOT NULL,
    "Category" VARCHAR(500) NULL,
    "Message" TEXT NULL,
    "Exception" TEXT NULL,
    "Environment" VARCHAR(100) NULL,
    "MachineName" VARCHAR(100) NULL,
    "ServiceName" VARCHAR(100) NULL,
    "CorrelationId" VARCHAR(100) NULL
);

-- Create index for faster queries
CREATE INDEX IF NOT EXISTS "IX_Logs_Timestamp" ON "Logging"."Logs" ("Timestamp" DESC);
CREATE INDEX IF NOT EXISTS "IX_Logs_LogLevel" ON "Logging"."Logs" ("LogLevel");
CREATE INDEX IF NOT EXISTS "IX_Logs_ServiceName" ON "Logging"."Logs" ("ServiceName");
CREATE INDEX IF NOT EXISTS "IX_Logs_CorrelationId" ON "Logging"."Logs" ("CorrelationId");
