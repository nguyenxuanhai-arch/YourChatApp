-- ============================================
-- FIX VIDEO CALL TABLE SCHEMA
-- Run this in MySQL Workbench or command line
-- ============================================

USE yourchatapp;

-- Drop the old table with wrong schema
DROP TABLE IF EXISTS VideoCallRequests;

-- Recreate with correct column names
CREATE TABLE VideoCallRequests (
    CallId VARCHAR(36) PRIMARY KEY,
    CallerId INT NOT NULL,
    CallerName VARCHAR(100) NOT NULL,
    ReceiverId INT NOT NULL,
    Status VARCHAR(20) DEFAULT 'pending',
    InitiatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    AcceptedAt TIMESTAMP NULL,
    RejectedAt TIMESTAMP NULL,
    FOREIGN KEY (CallerId) REFERENCES Users(UserId),
    FOREIGN KEY (ReceiverId) REFERENCES Users(UserId)
);

-- Verify the table structure
DESCRIBE VideoCallRequests;

SELECT 'Table VideoCallRequests recreated successfully!' AS Result;
