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

INSERT INTO Brands (Id, Name, Priority, Description, Created, Edited) VALUES (1, N'A350900XWB', 1, NULL, N'2022-09-28 23:59:52.0000000', N'2022-09-28 23:59:52.0000000');
INSERT INTO Brands (Id, Name, Priority, Description, Created, Edited) VALUES (2, N'A380800', 1, null, N'2022-09-28 23:59:57.0000000', N'2022-09-28 23:59:57.0000000');
INSERT INTO Brands (Id, Name, Priority, Description, Created, Edited) VALUES (3, N'Boeing777', 1, null, N'2022-09-29 00:00:05.0000000', N'2022-09-29 00:00:05.0000000');
INSERT INTO Brands (Id, Name, Priority, Description, Created, Edited) VALUES (4, N'LimitedEdition', 1, null, N'2022-09-29 00:00:13.0000000', N'2022-09-29 00:00:13.0000000');
INSERT INTO Brands (Id, Name, Priority, Description, Created, Edited) VALUES (5, N'BundleDeals', 1, null, N'2022-09-29 00:00:22.0000000', N'2022-09-29 00:00:22.0000000');

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

    INSERT INTO Categories (Id, Name, DisplayName, Description, ParentId) VALUES (1, N'Available', N'Available', NULL, NULL);
    INSERT INTO Categories (Id, Name, DisplayName, Description, ParentId) VALUES (2, N'In-Stock', N'In-Stock', NULL, NULL);
    INSERT INTO Categories (Id, Name, DisplayName, Description, ParentId) VALUES (3, N'Not-Available', N'Not-Available', NULL, NULL);

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

INSERT INTO Colors (Id, Name, Priority) VALUES (1, N'White', 1);
INSERT INTO Colors (Id, Name, Priority) VALUES (2, N'Black', 1);
INSERT INTO Colors (Id, Name, Priority) VALUES (3, N'Blue', 1);
INSERT INTO Colors (Id, Name, Priority) VALUES (4, N'Red', 1);

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

-- Insert statements
INSERT INTO Sizes (Id, Unit, Value) VALUES (1, N'US', 1.100);
INSERT INTO Sizes (Id, Unit, Value) VALUES (2, N'US', 1.200);
INSERT INTO Sizes (Id, Unit, Value) VALUES (3, N'US', 1.250);
INSERT INTO Sizes (Id, Unit, Value) VALUES (4, N'US', 1.400);
INSERT INTO Sizes (Id, Unit, Value) VALUES (5, N'US', 1.500);

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

INSERT INTO Airplanes (Id, Name, Priority, Active, Description, Features, Note, Created, Edited, BrandId, CategoryId) VALUES (1, N'A350900XWB Fresh Foam X 880v12 - Available', 1, 1, N'The New Balance Fresh Foam X 880v12 women''s running shoes provide ever-reliable support for you to reach your fitness goals. <br><br>A soft double jacquard mesh upper with a traditional lacing system ensures a secure and breathable fit. <br><br>The dual-density midsole with Fresh Foam X provides a plush feel right out-of-the-box and a responsive sensation on footstrike. <br><br>A grippy outsole offers road-ready durability for you to stay quick and comfortable on your feet from everyday runs to rigorous training on the road. <br><br>- Glide over any distance with the comfort and support you need to enjoy every run <br>- Double jacquard mesh upper for a structured yet airy feel <br>- Reflective elements<br>- Buttery-smooth landings and energetic toe-off from Fresh Foam X midsole <br>- Protected feel at the high-stress zones of the heel<br>- Ideal blend of durability and grip from the blown rubber outsole', N'<div class="row"><div class="small-4 columns"><strong>Support:</strong></div><div class="small-8 columns">Neutral</div></div><div class="row"><div class="small-4 columns"><strong>Upper:</strong></div><div class="small-8 columns">Mesh</div></div><div class="row"><div class="small-4 columns"><strong>Midsole:</strong></div><div class="small-8 columns">New Balance Fresh Foam X</div></div><div class="row"><div class="small-4 columns"><strong>Drop:</strong></div><div class="small-8 columns">10mm</div></div><div class="row"><div class="small-4 columns"><strong>Weight:</strong></div><div class="small-8 columns">237g / 8.4oz</div></div>', null, N'2022-09-28 23:59:52.0000000', N'2022-09-28 23:59:52.0000000', 1, 1);
INSERT INTO Airplanes (Id, Name, Priority, Active, Description, Features, Note, Created, Edited, BrandId, CategoryId) VALUES (2, N'A380800 Fresh Foam 880v11 - In-Stock', 1, 1, N'The New Balance Fresh Foam 880v11 women’s running shoes are your go-to for everyday training, providing all comfort and zero distractions. <br><br>A neutral running shoe you can trust in every stride, it keeps you motivated to lace up from its head-turning looks to its soft and springy ride.  <br><br>Wrapping your feet in a double-jacquard mesh upper, it provides a blend of high breathability and structure – keeping your active feet feeling cool and supported. It offers a soft, non-restrictive fit for a free-feeling sensation as you rule the road. <br><br>A moulded heel counter hugs securely to your rearfoot to reduce heel-slippage, ensuring a smooth and effortless feel. <br><br>Injected with Fresh Foam X, it delivers a confidence-inspiring ride with premium cushioning technologies in a lightweight package. It’s a winning fusion of softness and responsiveness to make easy work of your daily mileage.<br> <br>Softer blown rubber is strategically placed at the forefoot of the outsole. A denser rubber in the heel strike zone increases the lifespan of your ever-reliable running shoe. <br><br>- Your trustworthy ride to log daily mileage in comfort<br>- Upgraded with a breathable double-jacquard mesh upper for a more spacious forefoot and supportive midfoot fit<br>- Modern, textured upper design to turn heads<br>- Lacing system for a personalised, locked-in fit <br>- Moulded external heel counter secures your heel in place like a seatbelt <br>- Cradles your feet in the next level of comfort with a soft moulded footbed<br>- Rolls over the Fresh Foam X cushioning from NB’s premium running shoes for a plush yet responsive ride<br>- Strategic densities of outsole rubber for responsiveness and road-worthy durability - ensuring a long-term partnership you can count on', N'<div class="row"><div class="small-4 columns"><strong>Support:</strong></div><div class="small-8 columns">Neutral</div></div><div class="row"><div class="small-4 columns"><strong>Upper:</strong></div><div class="small-8 columns">Mesh</div></div><div class="row"><div class="small-4 columns"><strong>Midsole:</strong></div><div class="small-8 columns">New Balance Fresh Foam X</div></div><div class="row"><div class="small-4 columns"><strong>Drop:</strong></div><div class="small-8 columns">10mm</div></div><div class="row"><div class="small-4 columns"><strong>Weight:</strong></div><div class="small-8 columns">247g / 8.7oz</div></div>', null, N'2022-09-28 23:59:53.0000000', N'2022-09-28 23:59:53.0000000', 1, 1);
INSERT INTO Airplanes (Id, Name, Priority, Active, Description, Features, Note, Created, Edited, BrandId, CategoryId) VALUES (3, N'Boeing777 Fresh Foam 880v11 - Available', 1, 1, N'The New Balance Fresh Foam 880v11 men’s running shoes are your go-to for everyday training, providing all comfort and zero distractions. <br><br>A neutral running shoe you can trust in every stride, it keeps you motivated to lace up from its head-turning looks to its soft and springy ride.  <br><br>Wrapping your feet in a double-jacquard mesh upper, it provides a blend of high breathability and structure – keeping your active feet feeling cool and supported. It offers a soft, non-restrictive fit for a free-feeling sensation as you rule the road. <br><br>A moulded heel counter hugs securely to your rearfoot to reduce heel-slippage, ensuring a smooth and effortless feel. <br><br>Injected with Fresh Foam X, it delivers a confidence-inspiring ride with premium cushioning technologies in a lightweight package. It’s a winning fusion of softness and responsiveness to make easy work of your daily mileage.<br> <br>Softer blown rubber is strategically placed at the forefoot of the outsole. A denser rubber in the heel strike zone increases the lifespan of your ever-reliable running shoe. <br><br>- Your trustworthy ride to log daily mileage in comfort<br>- Upgraded with a breathable double-jacquard mesh upper for a more spacious forefoot and supportive midfoot fit<br>- Modern, textured upper design to turn heads<br>- Lacing system for a personalised, locked-in fit <br>- Moulded external heel counter secures your heel in place like a seatbelt <br>- Cradles your feet in the next level of comfort with a soft moulded footbed<br>- Rolls over the Fresh Foam X cushioning from NB’s premium running shoes for a plush yet responsive ride<br>- Strategic densities of outsole rubber for responsiveness and road-worthy durability - ensuring a long-term partnership you can count on', N'<div class="row"><div class="small-4 columns"><strong>Support:</strong></div><div class="small-8 columns">Neutral</div></div><div class="row"><div class="small-4 columns"><strong>Upper:</strong></div><div class="small-8 columns">Mesh</div></div><div class="row"><div class="small-4 columns"><strong>Midsole:</strong></div><div class="small-8 columns">New Balance Fresh Foam X</div></div><div class="row"><div class="small-4 columns"><strong>Drop:</strong></div><div class="small-8 columns">10mm</div></div><div class="row"><div class="small-4 columns"><strong>Weight:</strong></div><div class="small-8 columns">298g / 10.5oz</div></div>', null, N'2022-09-28 23:59:53.0000000', N'2022-09-28 23:59:53.0000000', 1, 2);
INSERT INTO Airplanes (Id, Name, Priority, Active, Description, Features, Note, Created, Edited, BrandId, CategoryId) VALUES (4, N'LimitedEdition 413 - In-Stock', 1, 1, N'The New Balance 413 men’s running shoes wrap your feet in a classic, everyday trainer without excess bulk. <br><br>The synthetic and mesh upper creates a lightweight ride without skimping on durability. You get versatile traction from the rubber outsole to take your runs from street to treadmill. <br><br>- Budget-friendly everyday running shoe<br>- Classic feel and athletic fit <br>- Lightweight synthetic/mesh upper <br>- Durable construction <br>- Saddle support <br>- Forefoot pod for step-in comfort<br>- Indoor and outdoor-ready grip from rubber outsole', N'<div class="row"><div class="small-4 columns"><strong>Support:</strong></div><div class="small-8 columns">Neutral</div></div><div class="row"><div class="small-4 columns"><strong>Upper:</strong></div><div class="small-8 columns">Mesh</div></div><div class="row"><div class="small-4 columns"><strong>Weight:</strong></div><div class="small-8 columns">212g / 7.5oz</div></div>', null, N'2022-09-28 23:59:56.0000000', N'2022-09-28 23:59:56.0000000', 1, 2);
INSERT INTO Airplanes (Id, Name, Priority, Active, Description, Features, Note, Created, Edited, BrandId, CategoryId) VALUES (5, N'BundleDeals Fresh Foam 1080v11 - Not-Available', 1, 1, N'Lace up in the next evolution of plush comfort with the New Balance Fresh Foam 1080v11 women’s running shoes. <br><br>Designed to give you a luxurious experience from heel-to-toe, this high mileage running shoe envelops your feet in an Engineered Hypoknit upper for a blend of support and softness. <br><br>It creates an adaptable fit with zonal stretch and sets the bar for breathability, allowing cool air to circulate. Now you can reach your runner’s high without heat and humidity to weigh you down. <br><br>The Ultra Heel design wraps your rearfoot like a seatbelt – allowing your best running shoes and feet to move in harmony, free of heel slippage.<br> <br>A data-driven midsole with Fresh Foam X fuses modern engineering with the iconic feel of its predecessor, the popular 1080v10. The springy, energy-returning sensation lives on in this fresh update. <br><br>All-up it translates to shock-absorbing cushioning in a lightweight package – so you can run free of fatigue for longer as you clock up the kilometres. <br><br>Built on a durable and road-worthy outsole, New Balance haven''t missed a single detail when it comes to your comfort and performance. <br><br>- Premium high mileage running shoes for soft comfort in every stride and every kilometre on the road<br>- Precision-driven engineering creates a running shoe that delivers comfort from every angle<br>- Versatile design - plenty of cushion for easy-going recovery runs and plenty of responsiveness and pop to shift the pace up a gear<br>- Stays true to the popular feel of the 1080v10 while fine-tuning the upper for an even more breathable fit <br>- Engineered Hypoknit upper for just-right softness and stretch<br>- Supportive and secure feel from bootie upper design <br>- Lacing system for a snug and personalised fit <br>- Elevates comfort with an Ortholite sockliner to cradle your hard-working feet<br>- Ultra Heel design creates a snug wrap around your rearfoot<br>- Lightweight Fresh Foam X cushioning dampens hard impacts to keep your feet feeling fresh and energised <br>- Voronoi hexagonal geometries in a head-turning laser pattern to stand out from the pack <br>- Durable rubber extends the lifespan of the outsole to tackle long runs', N'<div class="row"><div class="small-4 columns"><strong>Support:</strong></div><div class="small-8 columns">Neutral</div></div><div class="row"><div class="small-4 columns"><strong>Upper:</strong></div><div class="small-8 columns">Knit</div></div><div class="row"><div class="small-4 columns"><strong>Midsole:</strong></div><div class="small-8 columns">New Balance Fresh Foam X</div></div><div class="row"><div class="small-4 columns"><strong>Drop:</strong></div><div class="small-8 columns">8mm</div></div><div class="row"><div class="small-4 columns"><strong>Weight:</strong></div><div class="small-8 columns">230g / 8.1oz</div></div>', null, N'2022-09-28 23:59:57.0000000', N'2022-09-28 23:59:57.0000000', 1, 1);

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

INSERT INTO AirplaneColor (Id, ProductCode, FactoryPrice, SalePrice, SortOrder, Priority, Active, Url, Created, Edited, AirplaneId, ColorId) VALUES (1, N'W880M12-B', 123.45, 125.45, 1, 1, 1, N'new-balance-fresh-foam-x-880v12-womens-running-shoes-eclipse-vibrant-apricot-vibrant-pink', N'2022-09-28 23:59:52.0000000', N'2022-09-28 23:59:52.0000000', 1, 1);
INSERT INTO AirplaneColor (Id, ProductCode, FactoryPrice, SalePrice, SortOrder, Priority, Active, Url, Created, Edited, AirplaneId, ColorId) VALUES (2, N'W880B12-B', 123.45, 125.45, 1, 1, 1, N'new-balance-fresh-foam-x-880v12-womens-running-shoes-black-violet-haze-steel', N'2022-09-28 23:59:53.0000000', N'2022-09-28 23:59:53.0000000', 2, 2);
INSERT INTO AirplaneColor (Id, ProductCode, FactoryPrice, SalePrice, SortOrder, Priority, Active, Url, Created, Edited, AirplaneId, ColorId) VALUES (3, N'W880S11-B', 110.90, 112.90, 1, 1, 1, N'new-balance-fresh-foam-880v11-womens-running-shoes-light-cyclone-virtual-sky', N'2022-09-28 23:59:53.0000000', N'2022-09-28 23:59:53.0000000', 3, 3);
INSERT INTO AirplaneColor (Id, ProductCode, FactoryPrice, SalePrice, SortOrder, Priority, Active, Url, Created, Edited, AirplaneId, ColorId) VALUES (4, N'M880L11-D', 104.63, 106.63, 1, 1, 1, N'new-balance-fresh-foam-880v11-mens-running-shoes-black-cyclone', N'2022-09-28 23:59:53.0000000', N'2022-09-28 23:59:53.0000000',4, 4);
INSERT INTO AirplaneColor (Id, ProductCode, FactoryPrice, SalePrice, SortOrder, Priority, Active, Url, Created, Edited, AirplaneId, ColorId) VALUES (5, N'M880F11-D', 104.63, 106.63, 1, 1, 1, N'new-balance-fresh-foam-880v11-mens-running-shoes-wave-blue-virtual-sky', N'2022-09-28 23:59:53.0000000', N'2022-09-28 23:59:53.0000000', 5,1);

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

INSERT INTO Images (Id, Path, SortOrder, AirplaneColorId) VALUES (1, N'/images/shoes/881dd84f-87f7-42e3-9a07-5ef15d576197.jpg', 1, 1);
INSERT INTO Images (Id, Path, SortOrder, AirplaneColorId) VALUES (2, N'/images/shoes/c35dd48f-f32b-4174-b541-6596e485d831.jpg', 2, 2);
INSERT INTO Images (Id, Path, SortOrder, AirplaneColorId) VALUES (3, N'/images/shoes/d809222e-53ee-4389-912d-a9fa4b65a1f0.jpg', 3, 3);
INSERT INTO Images (Id, Path, SortOrder, AirplaneColorId) VALUES (4, N'/images/shoes/d1e750fc-f433-40c7-9dfb-0832777ce5f8.jpg', 4, 4);
INSERT INTO Images (Id, Path, SortOrder, AirplaneColorId) VALUES (5, N'/images/shoes/6c00a32c-0251-4439-bea8-92447755f1f5.jpg', 5, 1);


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


INSERT INTO AirplaneSize (Id, Quantity, AirplaneColorId, SizeId) VALUES (1, 5, 1, 1);
INSERT INTO AirplaneSize (Id, Quantity, AirplaneColorId, SizeId) VALUES (2, 10, 2, 2);
INSERT INTO AirplaneSize (Id, Quantity, AirplaneColorId, SizeId) VALUES (3, 15, 3, 3);
INSERT INTO AirplaneSize (Id, Quantity, AirplaneColorId, SizeId) VALUES (4, 10, 4, 4);
INSERT INTO AirplaneSize (Id, Quantity, AirplaneColorId, SizeId) VALUES (5,5, 1, 5);


SET IDENTITY_INSERT AirplaneSize OFF;
GO