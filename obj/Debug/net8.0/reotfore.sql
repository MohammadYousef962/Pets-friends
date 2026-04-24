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

CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO

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
GO

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ClientProfiles] (
    [Id] int NOT NULL IDENTITY,
    [UserAccountId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_ClientProfiles] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ClientProfiles_AspNetUsers_UserAccountId] FOREIGN KEY ([UserAccountId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [MerchantProfiles] (
    [Id] int NOT NULL IDENTITY,
    [UserAccountId] nvarchar(450) NOT NULL,
    [StoreName] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [ImageUrl] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_MerchantProfiles] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MerchantProfiles_AspNetUsers_UserAccountId] FOREIGN KEY ([UserAccountId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

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
GO

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
GO

CREATE TABLE [Orders] (
    [Id] int NOT NULL IDENTITY,
    [ClientProfileId] int NOT NULL,
    [OrderDate] datetime2 NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Orders] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Orders_ClientProfiles_ClientProfileId] FOREIGN KEY ([ClientProfileId]) REFERENCES [ClientProfiles] ([Id]) ON DELETE CASCADE
);
GO

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
GO

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
GO

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
GO

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
GO

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
GO

CREATE TABLE [CartItems] (
    [Id] int NOT NULL IDENTITY,
    [ClientProfileId] int NOT NULL,
    [ProductId] int NOT NULL,
    [Quantity] int NOT NULL,
    CONSTRAINT [PK_CartItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CartItems_ClientProfiles_ClientProfileId] FOREIGN KEY ([ClientProfileId]) REFERENCES [ClientProfiles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_CartItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id])
);
GO

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
GO

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
GO

CREATE INDEX [IX_Appointments_ClientProfileId] ON [Appointments] ([ClientProfileId]);
GO

CREATE INDEX [IX_Appointments_PetId] ON [Appointments] ([PetId]);
GO

CREATE INDEX [IX_Appointments_ServiceId] ON [Appointments] ([ServiceId]);
GO

CREATE INDEX [IX_Appointments_VetProfileId] ON [Appointments] ([VetProfileId]);
GO

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO

CREATE INDEX [IX_CartItems_ClientProfileId] ON [CartItems] ([ClientProfileId]);
GO

CREATE INDEX [IX_CartItems_ProductId] ON [CartItems] ([ProductId]);
GO

CREATE UNIQUE INDEX [IX_ClientProfiles_UserAccountId] ON [ClientProfiles] ([UserAccountId]);
GO

CREATE UNIQUE INDEX [IX_MerchantProfiles_UserAccountId] ON [MerchantProfiles] ([UserAccountId]);
GO

CREATE INDEX [IX_OrderItems_OrderId] ON [OrderItems] ([OrderId]);
GO

CREATE INDEX [IX_OrderItems_ProductId] ON [OrderItems] ([ProductId]);
GO

CREATE INDEX [IX_Orders_ClientProfileId] ON [Orders] ([ClientProfileId]);
GO

CREATE INDEX [IX_Pets_ClientProfileId] ON [Pets] ([ClientProfileId]);
GO

CREATE INDEX [IX_Products_MerchantProfileId] ON [Products] ([MerchantProfileId]);
GO

CREATE INDEX [IX_Services_ShelterProfileId] ON [Services] ([ShelterProfileId]);
GO

CREATE INDEX [IX_Services_VetProfileId] ON [Services] ([VetProfileId]);
GO

CREATE UNIQUE INDEX [IX_ShelterProfiles_UserAccountId] ON [ShelterProfiles] ([UserAccountId]);
GO

CREATE UNIQUE INDEX [IX_VetProfiles_UserAccountId] ON [VetProfiles] ([UserAccountId]);
GO

CREATE INDEX [IX_VetReviews_ReviewerId] ON [VetReviews] ([ReviewerId]);
GO

CREATE INDEX [IX_VetReviews_VetProfileId] ON [VetReviews] ([VetProfileId]);
GO

CREATE INDEX [IX_WorkingDays_ShelterProfileId] ON [WorkingDays] ([ShelterProfileId]);
GO

CREATE INDEX [IX_WorkingDays_VetProfileId] ON [WorkingDays] ([VetProfileId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260423124332_InitialCreate', N'8.0.0');
GO

COMMIT;
GO

