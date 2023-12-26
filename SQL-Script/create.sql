IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'Aircraft')
    CREATE DATABASE Aircraft;
GO

-- Now switch to the Aircraft database
USE Aircraft;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Brands')
BEGIN
    CREATE TABLE Brands
    (
        Id          INT IDENTITY
            CONSTRAINT PK_Brands PRIMARY KEY,
        Name        NVARCHAR(MAX) NOT NULL,
        Priority    INT NOT NULL,
        Description NVARCHAR(MAX),
        Created     DATETIME2 NOT NULL,
        Edited      DATETIME2 NOT NULL
    );
END
GO
-- Insert data into Brands
SET IDENTITY_INSERT Brands ON;


SET IDENTITY_INSERT Brands OFF;
GO

-- Create the Categories table if it does not exist
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Categories')
BEGIN
    CREATE TABLE Categories
    (
        Id          INT IDENTITY
            CONSTRAINT PK_Categories PRIMARY KEY,
        Name        NVARCHAR(MAX) NOT NULL,
        DisplayName NVARCHAR(MAX),
        Description NVARCHAR(MAX),
        ParentId    INT
            CONSTRAINT FK_Categories_Categories_ParentId
                REFERENCES Categories
    );
    CREATE INDEX IX_Categories_ParentId ON Categories (ParentId);
END
GO

-- Enable IDENTITY_INSERT and perform the insert operations in the same batch
BEGIN
    SET IDENTITY_INSERT Categories ON;

  
    SET IDENTITY_INSERT Categories OFF;
END
GO



IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Colors')
BEGIN
    CREATE TABLE Colors
    (
        Id INT IDENTITY
            CONSTRAINT PK_Colors PRIMARY KEY,
        Name NVARCHAR(MAX) NOT NULL,
        Priority INT NOT NULL
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Colors')
BEGIN
    CREATE TABLE Colors
    (
        Id INT IDENTITY
            CONSTRAINT PK_Colors PRIMARY KEY,
        Name NVARCHAR(MAX) NOT NULL,
        Priority INT NOT NULL
    );
END
GO

-- Ensure IDENTITY_INSERT is ON before inserting values
SET IDENTITY_INSERT Colors ON;

-- Turn it OFF after inserting values
SET IDENTITY_INSERT Colors OFF;
GO



IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Sizes')
BEGIN
    CREATE TABLE Sizes
    (
        Id INT IDENTITY
            CONSTRAINT PK_Sizes PRIMARY KEY,
        Unit NVARCHAR(MAX) NOT NULL,
        Value FLOAT NOT NULL
    );
END
GO

-- Enable IDENTITY_INSERT for Sizes
SET IDENTITY_INSERT Sizes ON;



-- Disable IDENTITY_INSERT after the insertions
SET IDENTITY_INSERT Sizes OFF;
GO



IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Airplanes')
BEGIN
    CREATE TABLE Airplanes
    (
        Id INT IDENTITY
            CONSTRAINT PK_Airplanes PRIMARY KEY,
        Name NVARCHAR(MAX) NOT NULL,
        Priority INT NOT NULL,
        Active BIT NOT NULL,
        Description NVARCHAR(MAX),
        Features NVARCHAR(MAX),
        Note NVARCHAR(MAX),
        Created DATETIME2 NOT NULL,
        Edited DATETIME2 NOT NULL,
        BrandId INT
            CONSTRAINT FK_Airplanes_Brands_BrandId REFERENCES Brands,
        CategoryId INT
            CONSTRAINT FK_Airplanes_Categories_CategoryId REFERENCES Categories
    );

    CREATE INDEX IX_Airplanes_BrandId ON Airplanes (BrandId);
    CREATE INDEX IX_Airplanes_CategoryId ON Airplanes (CategoryId);
END
GO

-- Enable IDENTITY_INSERT for Airplanes
SET IDENTITY_INSERT Airplanes ON;


SET IDENTITY_INSERT Airplanes OFF;
GO


IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AirplaneColor')
BEGIN
    CREATE TABLE AirplaneColor
    (
        Id INT IDENTITY
            CONSTRAINT PK_AirplaneColor PRIMARY KEY,
        ProductCode NVARCHAR(MAX) NOT NULL,
        FactoryPrice DECIMAL(10, 2) NOT NULL,
        SalePrice DECIMAL(10, 2) NOT NULL,
        SortOrder INT NOT NULL,
        Priority INT NOT NULL,
        Active BIT NOT NULL,
        Url NVARCHAR(MAX),
        Created DATETIME2 NOT NULL,
        Edited DATETIME2 NOT NULL,
        AirplaneId INT NOT NULL
            CONSTRAINT FK_AirplaneColor_Airplanes_AirplaneId
                REFERENCES Airplanes ON DELETE CASCADE,
        ColorId INT NOT NULL
            CONSTRAINT FK_AirplaneColor_Colors_ColorId
                REFERENCES Colors ON DELETE CASCADE
    );

    CREATE INDEX IX_AirplaneColor_ColorId ON AirplaneColor (ColorId);
    CREATE INDEX IX_AirplaneColor_AirplaneId ON AirplaneColor (AirplaneId);
END
GO

-- Enable IDENTITY_INSERT for AirplaneColor
SET IDENTITY_INSERT AirplaneColor ON;


SET IDENTITY_INSERT AirplaneColor OFF;
GO



IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Images')
BEGIN
    CREATE TABLE Images
    (
        Id INT IDENTITY
            CONSTRAINT PK_Images PRIMARY KEY,
        Path NVARCHAR(MAX) NOT NULL,
        SortOrder INT NOT NULL,
        AirplaneColorId INT NOT NULL
            CONSTRAINT FK_Images_AirplaneColor_AirplaneColorId
                REFERENCES AirplaneColor ON DELETE CASCADE
    );

    CREATE INDEX IX_Images_AirplaneColorId ON Images (AirplaneColorId);
END
GO

-- Enable IDENTITY_INSERT for Images
SET IDENTITY_INSERT Images ON;


SET IDENTITY_INSERT Images OFF;
GO


IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AirplaneSize')
BEGIN
    CREATE TABLE AirplaneSize
    (
        Id INT IDENTITY
            CONSTRAINT PK_AirplaneSize PRIMARY KEY,
        Quantity INT NOT NULL,
        AirplaneColorId INT NOT NULL
            CONSTRAINT FK_AirplaneSize_AirplaneColor_AirplaneColorId
                REFERENCES AirplaneColor ON DELETE CASCADE,
        SizeId INT NOT NULL
            CONSTRAINT FK_AirplaneSize_Sizes_SizeId
                REFERENCES Sizes ON DELETE CASCADE
    );

    CREATE INDEX IX_AirplaneSize_AirplaneColorId ON AirplaneSize (AirplaneColorId);
    CREATE INDEX IX_AirplaneSize_SizeId ON AirplaneSize (SizeId);
END
GO

-- Enable IDENTITY_INSERT for AirplaneSize
SET IDENTITY_INSERT AirplaneSize ON;



SET IDENTITY_INSERT AirplaneSize OFF;
GO