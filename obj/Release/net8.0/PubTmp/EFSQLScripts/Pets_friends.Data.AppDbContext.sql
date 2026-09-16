IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [FullName] nvarchar(max) NOT NULL,
        [Gender] nvarchar(max) NULL,
        [City] nvarchar(max) NULL,
        [IsProfileComplete] bit NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE TABLE [ClientProfiles] (
        [Id] int NOT NULL IDENTITY,
        [UserAccountId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_ClientProfiles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ClientProfiles_AspNetUsers_UserAccountId] FOREIGN KEY ([UserAccountId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE TABLE [MerchantProfiles] (
        [Id] int NOT NULL IDENTITY,
        [UserAccountId] nvarchar(450) NOT NULL,
        [StoreName] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [ImageUrl] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_MerchantProfiles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MerchantProfiles_AspNetUsers_UserAccountId] FOREIGN KEY ([UserAccountId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE TABLE [ShelterProfiles] (
        [Id] int NOT NULL IDENTITY,
        [UserAccountId] nvarchar(450) NOT NULL,
        [ShelterName] nvarchar(max) NOT NULL,
        [ShelterAddress] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [ImageUrl] nvarchar(max) NOT NULL,
        [Services] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_ShelterProfiles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ShelterProfiles_AspNetUsers_UserAccountId] FOREIGN KEY ([UserAccountId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE TABLE [VetProfiles] (
        [Id] int NOT NULL IDENTITY,
        [UserAccountId] nvarchar(450) NOT NULL,
        [Specialization] nvarchar(max) NOT NULL,
        [ClinicName] nvarchar(max) NOT NULL,
        [ClinicAddress] nvarchar(max) NOT NULL,
        [YearsOfExperience] int NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [ImageUrl] nvarchar(max) NOT NULL,
        [Services] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_VetProfiles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_VetProfiles_AspNetUsers_UserAccountId] FOREIGN KEY ([UserAccountId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE TABLE [Orders] (
        [Id] int NOT NULL IDENTITY,
        [ClientProfileId] int NOT NULL,
        [OrderDate] datetime2 NOT NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Orders] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Orders_ClientProfiles_ClientProfileId] FOREIGN KEY ([ClientProfileId]) REFERENCES [ClientProfiles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE TABLE [Pets] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [Species] nvarchar(max) NOT NULL,
        [Breed] nvarchar(max) NOT NULL,
        [Age] int NOT NULL,
        [ClientProfileId] int NOT NULL,
        CONSTRAINT [PK_Pets] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Pets_ClientProfiles_ClientProfileId] FOREIGN KEY ([ClientProfileId]) REFERENCES [ClientProfiles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE TABLE [Products] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Category] nvarchar(max) NOT NULL,
        [Price] decimal(18,2) NOT NULL,
        [MerchantProfileId] int NOT NULL,
        CONSTRAINT [PK_Products] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Products_MerchantProfiles_MerchantProfileId] FOREIGN KEY ([MerchantProfileId]) REFERENCES [MerchantProfiles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE TABLE [Services] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Price] decimal(18,2) NOT NULL,
        [VetProfileId] int NULL,
        [ShelterProfileId] int NULL,
        CONSTRAINT [PK_Services] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Services_ShelterProfiles_ShelterProfileId] FOREIGN KEY ([ShelterProfileId]) REFERENCES [ShelterProfiles] ([Id]),
        CONSTRAINT [FK_Services_VetProfiles_VetProfileId] FOREIGN KEY ([VetProfileId]) REFERENCES [VetProfiles] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE TABLE [VetReviews] (
        [Id] int NOT NULL IDENTITY,
        [Rating] int NOT NULL,
        [Comment] nvarchar(500) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [VetProfileId] int NOT NULL,
        [ReviewerId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_VetReviews] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_VetReviews_AspNetUsers_ReviewerId] FOREIGN KEY ([ReviewerId]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_VetReviews_VetProfiles_VetProfileId] FOREIGN KEY ([VetProfileId]) REFERENCES [VetProfiles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE TABLE [WorkingDays] (
        [Id] int NOT NULL IDENTITY,
        [Day] int NOT NULL,
        [OpenTime] time NULL,
        [CloseTime] time NULL,
        [IsOff] bit NOT NULL,
        [VetProfileId] int NULL,
        [ShelterProfileId] int NULL,
        CONSTRAINT [PK_WorkingDays] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WorkingDays_ShelterProfiles_ShelterProfileId] FOREIGN KEY ([ShelterProfileId]) REFERENCES [ShelterProfiles] ([Id]),
        CONSTRAINT [FK_WorkingDays_VetProfiles_VetProfileId] FOREIGN KEY ([VetProfileId]) REFERENCES [VetProfiles] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE TABLE [CartItems] (
        [Id] int NOT NULL IDENTITY,
        [ClientProfileId] int NOT NULL,
        [ProductId] int NOT NULL,
        [Quantity] int NOT NULL,
        CONSTRAINT [PK_CartItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CartItems_ClientProfiles_ClientProfileId] FOREIGN KEY ([ClientProfileId]) REFERENCES [ClientProfiles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CartItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE TABLE [OrderItems] (
        [Id] int NOT NULL IDENTITY,
        [OrderId] int NOT NULL,
        [ProductId] int NOT NULL,
        [Quantity] int NOT NULL,
        [Price] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_OrderItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrderItems_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_OrderItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE TABLE [Appointments] (
        [Id] int NOT NULL IDENTITY,
        [ClientProfileId] int NOT NULL,
        [PetId] int NOT NULL,
        [ServiceId] int NOT NULL,
        [VetProfileId] int NOT NULL,
        [AppointmentDate] datetime2 NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Appointments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Appointments_ClientProfiles_ClientProfileId] FOREIGN KEY ([ClientProfileId]) REFERENCES [ClientProfiles] ([Id]),
        CONSTRAINT [FK_Appointments_Pets_PetId] FOREIGN KEY ([PetId]) REFERENCES [Pets] ([Id]),
        CONSTRAINT [FK_Appointments_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([Id]),
        CONSTRAINT [FK_Appointments_VetProfiles_VetProfileId] FOREIGN KEY ([VetProfileId]) REFERENCES [VetProfiles] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Appointments_ClientProfileId] ON [Appointments] ([ClientProfileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Appointments_PetId] ON [Appointments] ([PetId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Appointments_ServiceId] ON [Appointments] ([ServiceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Appointments_VetProfileId] ON [Appointments] ([VetProfileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CartItems_ClientProfileId] ON [CartItems] ([ClientProfileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CartItems_ProductId] ON [CartItems] ([ProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ClientProfiles_UserAccountId] ON [ClientProfiles] ([UserAccountId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_MerchantProfiles_UserAccountId] ON [MerchantProfiles] ([UserAccountId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OrderItems_OrderId] ON [OrderItems] ([OrderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OrderItems_ProductId] ON [OrderItems] ([ProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Orders_ClientProfileId] ON [Orders] ([ClientProfileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Pets_ClientProfileId] ON [Pets] ([ClientProfileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Products_MerchantProfileId] ON [Products] ([MerchantProfileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Services_ShelterProfileId] ON [Services] ([ShelterProfileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Services_VetProfileId] ON [Services] ([VetProfileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ShelterProfiles_UserAccountId] ON [ShelterProfiles] ([UserAccountId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_VetProfiles_UserAccountId] ON [VetProfiles] ([UserAccountId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_VetReviews_ReviewerId] ON [VetReviews] ([ReviewerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_VetReviews_VetProfileId] ON [VetReviews] ([VetProfileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WorkingDays_ShelterProfileId] ON [WorkingDays] ([ShelterProfileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WorkingDays_VetProfileId] ON [WorkingDays] ([VetProfileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260423124332_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260423124332_InitialCreate', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260508081950_AddedStoreAddressToMerchant'
)
BEGIN
    ALTER TABLE [MerchantProfiles] ADD [StoreAddress] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260508081950_AddedStoreAddressToMerchant'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260508081950_AddedStoreAddressToMerchant', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260508090756_AddStockToProduct'
)
BEGIN
    ALTER TABLE [Products] ADD [StockQuantity] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260508090756_AddStockToProduct'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260508090756_AddStockToProduct', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509150538_AddProductImagesAndReviews'
)
BEGIN
    ALTER TABLE [Products] ADD [ImageUrl] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509150538_AddProductImagesAndReviews'
)
BEGIN
    CREATE TABLE [ProductReviews] (
        [Id] int NOT NULL IDENTITY,
        [ProductId] int NOT NULL,
        [ClientProfileId] int NOT NULL,
        [Rating] int NOT NULL,
        [Comment] nvarchar(1000) NOT NULL,
        [ReviewDate] datetime2 NOT NULL,
        CONSTRAINT [PK_ProductReviews] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProductReviews_ClientProfiles_ClientProfileId] FOREIGN KEY ([ClientProfileId]) REFERENCES [ClientProfiles] ([Id]),
        CONSTRAINT [FK_ProductReviews_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509150538_AddProductImagesAndReviews'
)
BEGIN
    CREATE INDEX [IX_ProductReviews_ClientProfileId] ON [ProductReviews] ([ClientProfileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509150538_AddProductImagesAndReviews'
)
BEGIN
    CREATE INDEX [IX_ProductReviews_ProductId] ON [ProductReviews] ([ProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260509150538_AddProductImagesAndReviews'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260509150538_AddProductImagesAndReviews', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260510125055_LinkOrdersToMerchant'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Orders]') AND [c].[name] = N'Status');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Orders] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [Orders] ALTER COLUMN [Status] nvarchar(50) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260510125055_LinkOrdersToMerchant'
)
BEGIN
    ALTER TABLE [Orders] ADD [MerchantProfileId] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260510125055_LinkOrdersToMerchant'
)
BEGIN
    CREATE INDEX [IX_Orders_MerchantProfileId] ON [Orders] ([MerchantProfileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260510125055_LinkOrdersToMerchant'
)
BEGIN
    ALTER TABLE [Orders] ADD CONSTRAINT [FK_Orders_MerchantProfiles_MerchantProfileId] FOREIGN KEY ([MerchantProfileId]) REFERENCES [MerchantProfiles] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260510125055_LinkOrdersToMerchant'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260510125055_LinkOrdersToMerchant', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260510164445_AddCartTable'
)
BEGIN
    CREATE TABLE [ShoppingCarts] (
        [Id] int NOT NULL IDENTITY,
        [ProductId] int NOT NULL,
        [UserAccountId] nvarchar(max) NOT NULL,
        [Quantity] int NOT NULL,
        CONSTRAINT [PK_ShoppingCarts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ShoppingCarts_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260510164445_AddCartTable'
)
BEGIN
    CREATE INDEX [IX_ShoppingCarts_ProductId] ON [ShoppingCarts] ([ProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260510164445_AddCartTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260510164445_AddCartTable', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511141130_AddCartAndUnitPrice'
)
BEGIN
    EXEC sp_rename N'[OrderItems].[Price]', N'UnitPrice', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511141130_AddCartAndUnitPrice'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260511141130_AddCartAndUnitPrice', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511141444_SetupECommerceTables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260511141444_SetupECommerceTables', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511161850_EnsureCompleteSchema'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260511161850_EnsureCompleteSchema', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260514060235_PushMissingLiveColumns'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Pets]') AND [c].[name] = N'Species');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Pets] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Pets] DROP COLUMN [Species];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260514060235_PushMissingLiveColumns'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Pets]') AND [c].[name] = N'Name');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Pets] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [Pets] ALTER COLUMN [Name] nvarchar(100) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260514060235_PushMissingLiveColumns'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Pets]') AND [c].[name] = N'Breed');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Pets] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [Pets] ALTER COLUMN [Breed] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260514060235_PushMissingLiveColumns'
)
BEGIN
    ALTER TABLE [Pets] ADD [Gender] nvarchar(20) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260514060235_PushMissingLiveColumns'
)
BEGIN
    ALTER TABLE [Pets] ADD [MedicalHistory] nvarchar(2000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260514060235_PushMissingLiveColumns'
)
BEGIN
    ALTER TABLE [Pets] ADD [ProfilePicture] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260514060235_PushMissingLiveColumns'
)
BEGIN
    ALTER TABLE [ClientProfiles] ADD [Address] nvarchar(250) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260514060235_PushMissingLiveColumns'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260514060235_PushMissingLiveColumns', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260515143035_AddShelterAndBoardingLogic'
)
BEGIN
    EXEC sp_rename N'[Pets].[ProfilePicture]', N'ImageUrl', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260515143035_AddShelterAndBoardingLogic'
)
BEGIN
    ALTER TABLE [Pets] ADD [IsAdopted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260515143035_AddShelterAndBoardingLogic'
)
BEGIN
    ALTER TABLE [Pets] ADD [ShelterProfileId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260515143035_AddShelterAndBoardingLogic'
)
BEGIN
    CREATE TABLE [AdoptionApplications] (
        [Id] int NOT NULL IDENTITY,
        [PetId] int NOT NULL,
        [ClientProfileId] int NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [Type] nvarchar(max) NOT NULL,
        [ApplicationDate] datetime2 NOT NULL,
        CONSTRAINT [PK_AdoptionApplications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AdoptionApplications_ClientProfiles_ClientProfileId] FOREIGN KEY ([ClientProfileId]) REFERENCES [ClientProfiles] ([Id]),
        CONSTRAINT [FK_AdoptionApplications_Pets_PetId] FOREIGN KEY ([PetId]) REFERENCES [Pets] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260515143035_AddShelterAndBoardingLogic'
)
BEGIN
    CREATE TABLE [BoardingRecords] (
        [Id] int NOT NULL IDENTITY,
        [ShelterProfileId] int NOT NULL,
        [PetName] nvarchar(max) NOT NULL,
        [PetBreed] nvarchar(max) NOT NULL,
        [OwnerName] nvarchar(max) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [TimeLabel] nvarchar(max) NOT NULL,
        [SpecialNotes] nvarchar(max) NOT NULL,
        [ScheduledDate] datetime2 NOT NULL,
        CONSTRAINT [PK_BoardingRecords] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BoardingRecords_ShelterProfiles_ShelterProfileId] FOREIGN KEY ([ShelterProfileId]) REFERENCES [ShelterProfiles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260515143035_AddShelterAndBoardingLogic'
)
BEGIN
    CREATE INDEX [IX_Pets_ShelterProfileId] ON [Pets] ([ShelterProfileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260515143035_AddShelterAndBoardingLogic'
)
BEGIN
    CREATE INDEX [IX_AdoptionApplications_ClientProfileId] ON [AdoptionApplications] ([ClientProfileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260515143035_AddShelterAndBoardingLogic'
)
BEGIN
    CREATE INDEX [IX_AdoptionApplications_PetId] ON [AdoptionApplications] ([PetId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260515143035_AddShelterAndBoardingLogic'
)
BEGIN
    CREATE INDEX [IX_BoardingRecords_ShelterProfileId] ON [BoardingRecords] ([ShelterProfileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260515143035_AddShelterAndBoardingLogic'
)
BEGIN
    ALTER TABLE [Pets] ADD CONSTRAINT [FK_Pets_ShelterProfiles_ShelterProfileId] FOREIGN KEY ([ShelterProfileId]) REFERENCES [ShelterProfiles] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260515143035_AddShelterAndBoardingLogic'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260515143035_AddShelterAndBoardingLogic', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260516072048_AddShelterProfileColumns'
)
BEGIN
    EXEC sp_rename N'[ShelterProfiles].[Services]', N'Address', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260516072048_AddShelterProfileColumns'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260516072048_AddShelterProfileColumns', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260516074239_AddShelterServicesColumn'
)
BEGIN
    ALTER TABLE [ShelterProfiles] ADD [Services] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260516074239_AddShelterServicesColumn'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260516074239_AddShelterServicesColumn', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260516075440_RemoveShelterServices'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ShelterProfiles]') AND [c].[name] = N'Services');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [ShelterProfiles] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [ShelterProfiles] DROP COLUMN [Services];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260516075440_RemoveShelterServices'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260516075440_RemoveShelterServices', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260516154003_UpdatePetModel'
)
BEGIN
    ALTER TABLE [Pets] DROP CONSTRAINT [FK_Pets_ClientProfiles_ClientProfileId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260516154003_UpdatePetModel'
)
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Pets]') AND [c].[name] = N'Age');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Pets] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [Pets] DROP COLUMN [Age];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260516154003_UpdatePetModel'
)
BEGIN
    DECLARE @var6 sysname;
    SELECT @var6 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Pets]') AND [c].[name] = N'Name');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Pets] DROP CONSTRAINT [' + @var6 + '];');
    ALTER TABLE [Pets] ALTER COLUMN [Name] nvarchar(max) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260516154003_UpdatePetModel'
)
BEGIN
    DECLARE @var7 sysname;
    SELECT @var7 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Pets]') AND [c].[name] = N'MedicalHistory');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Pets] DROP CONSTRAINT [' + @var7 + '];');
    ALTER TABLE [Pets] ALTER COLUMN [MedicalHistory] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260516154003_UpdatePetModel'
)
BEGIN
    DECLARE @var8 sysname;
    SELECT @var8 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Pets]') AND [c].[name] = N'Gender');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [Pets] DROP CONSTRAINT [' + @var8 + '];');
    EXEC(N'UPDATE [Pets] SET [Gender] = N'''' WHERE [Gender] IS NULL');
    ALTER TABLE [Pets] ALTER COLUMN [Gender] nvarchar(max) NOT NULL;
    ALTER TABLE [Pets] ADD DEFAULT N'' FOR [Gender];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260516154003_UpdatePetModel'
)
BEGIN
    DECLARE @var9 sysname;
    SELECT @var9 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Pets]') AND [c].[name] = N'ClientProfileId');
    IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [Pets] DROP CONSTRAINT [' + @var9 + '];');
    ALTER TABLE [Pets] ALTER COLUMN [ClientProfileId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260516154003_UpdatePetModel'
)
BEGIN
    DECLARE @var10 sysname;
    SELECT @var10 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Pets]') AND [c].[name] = N'Breed');
    IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [Pets] DROP CONSTRAINT [' + @var10 + '];');
    EXEC(N'UPDATE [Pets] SET [Breed] = N'''' WHERE [Breed] IS NULL');
    ALTER TABLE [Pets] ALTER COLUMN [Breed] nvarchar(max) NOT NULL;
    ALTER TABLE [Pets] ADD DEFAULT N'' FOR [Breed];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260516154003_UpdatePetModel'
)
BEGIN
    ALTER TABLE [Pets] ADD [DateOfBirth] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260516154003_UpdatePetModel'
)
BEGIN
    ALTER TABLE [Pets] ADD [Description] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260516154003_UpdatePetModel'
)
BEGIN
    ALTER TABLE [Pets] ADD [IsNeutered] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260516154003_UpdatePetModel'
)
BEGIN
    ALTER TABLE [Pets] ADD [IsPubliclyListed] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260516154003_UpdatePetModel'
)
BEGIN
    ALTER TABLE [Pets] ADD CONSTRAINT [FK_Pets_ClientProfiles_ClientProfileId] FOREIGN KEY ([ClientProfileId]) REFERENCES [ClientProfiles] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260516154003_UpdatePetModel'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260516154003_UpdatePetModel', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517052523_AddAppointmentNotesAndUrgent'
)
BEGIN
    DECLARE @var11 sysname;
    SELECT @var11 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Appointments]') AND [c].[name] = N'ServiceId');
    IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [Appointments] DROP CONSTRAINT [' + @var11 + '];');
    ALTER TABLE [Appointments] ALTER COLUMN [ServiceId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517052523_AddAppointmentNotesAndUrgent'
)
BEGIN
    ALTER TABLE [Appointments] ADD [IsUrgent] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517052523_AddAppointmentNotesAndUrgent'
)
BEGIN
    ALTER TABLE [Appointments] ADD [Notes] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517052523_AddAppointmentNotesAndUrgent'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260517052523_AddAppointmentNotesAndUrgent', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517071122_MakeAppointmentPetIdNullable'
)
BEGIN
    DECLARE @var12 sysname;
    SELECT @var12 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Appointments]') AND [c].[name] = N'PetId');
    IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [Appointments] DROP CONSTRAINT [' + @var12 + '];');
    ALTER TABLE [Appointments] ALTER COLUMN [PetId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517071122_MakeAppointmentPetIdNullable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260517071122_MakeAppointmentPetIdNullable', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517142359_AddClientProfileImage'
)
BEGIN
    ALTER TABLE [ClientProfiles] ADD [ImageUrl] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517142359_AddClientProfileImage'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260517142359_AddClientProfileImage', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517160718_AddedPetNameLockAndClientImage'
)
BEGIN
    ALTER TABLE [Pets] ADD [LastNameChangeDate] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260517160718_AddedPetNameLockAndClientImage'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260517160718_AddedPetNameLockAndClientImage', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260518124900_AddedAdoptionRequestTable'
)
BEGIN
    CREATE TABLE [AdoptionRequests] (
        [Id] int NOT NULL IDENTITY,
        [PetId] int NOT NULL,
        [ShelterProfileId] int NOT NULL,
        [ClientProfileId] int NOT NULL,
        [ApplicantId] nvarchar(max) NOT NULL,
        [Motivation] nvarchar(max) NOT NULL,
        [LivingSituation] nvarchar(max) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [RequestDate] datetime2 NOT NULL,
        CONSTRAINT [PK_AdoptionRequests] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260518124900_AddedAdoptionRequestTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260518124900_AddedAdoptionRequestTable', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529115008_AddPickUpDate'
)
BEGIN
    ALTER TABLE [BoardingRecords] ADD [PickUpDate] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529115008_AddPickUpDate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260529115008_AddPickUpDate', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529133247_AutoDeleteAppointmentsWithPet'
)
BEGIN
    ALTER TABLE [AdoptionApplications] DROP CONSTRAINT [FK_AdoptionApplications_Pets_PetId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529133247_AutoDeleteAppointmentsWithPet'
)
BEGIN
    ALTER TABLE [Appointments] DROP CONSTRAINT [FK_Appointments_Pets_PetId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529133247_AutoDeleteAppointmentsWithPet'
)
BEGIN
    ALTER TABLE [AdoptionApplications] ADD CONSTRAINT [FK_AdoptionApplications_Pets_PetId] FOREIGN KEY ([PetId]) REFERENCES [Pets] ([Id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529133247_AutoDeleteAppointmentsWithPet'
)
BEGIN
    ALTER TABLE [Appointments] ADD CONSTRAINT [FK_Appointments_Pets_PetId] FOREIGN KEY ([PetId]) REFERENCES [Pets] ([Id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529133247_AutoDeleteAppointmentsWithPet'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260529133247_AutoDeleteAppointmentsWithPet', N'8.0.0');
END;
GO

COMMIT;
GO

