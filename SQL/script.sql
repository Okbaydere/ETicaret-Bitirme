USE [master]
GO
/****** Object:  Database [ETicaret]    Script Date: 3/24/2025 12:24:06 AM ******/
CREATE DATABASE [ETicaret]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'ETicaret', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\ETicaret.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'ETicaret_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\ETicaret_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [ETicaret] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [ETicaret].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [ETicaret] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [ETicaret] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [ETicaret] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [ETicaret] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [ETicaret] SET ARITHABORT OFF 
GO
ALTER DATABASE [ETicaret] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [ETicaret] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [ETicaret] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [ETicaret] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [ETicaret] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [ETicaret] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [ETicaret] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [ETicaret] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [ETicaret] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [ETicaret] SET  ENABLE_BROKER 
GO
ALTER DATABASE [ETicaret] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [ETicaret] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [ETicaret] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [ETicaret] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [ETicaret] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [ETicaret] SET READ_COMMITTED_SNAPSHOT ON 
GO
ALTER DATABASE [ETicaret] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [ETicaret] SET RECOVERY FULL 
GO
ALTER DATABASE [ETicaret] SET  MULTI_USER 
GO
ALTER DATABASE [ETicaret] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [ETicaret] SET DB_CHAINING OFF 
GO
ALTER DATABASE [ETicaret] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [ETicaret] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [ETicaret] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [ETicaret] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
EXEC sys.sp_db_vardecimal_storage_format N'ETicaret', N'ON'
GO
ALTER DATABASE [ETicaret] SET QUERY_STORE = ON
GO
ALTER DATABASE [ETicaret] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [ETicaret]
GO
/****** Object:  Table [dbo].[__EFMigrationsHistory]    Script Date: 3/24/2025 12:24:06 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Addresses]    Script Date: 3/24/2025 12:24:06 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Addresses](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [nvarchar](max) NOT NULL,
	[FullAddress] [nvarchar](max) NOT NULL,
	[City] [nvarchar](max) NOT NULL,
	[UserId] [int] NOT NULL,
	[IsDefault] [bit] NOT NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_Addresses] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetRoleClaims]    Script Date: 3/24/2025 12:24:06 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoleClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RoleId] [int] NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetRoles]    Script Date: 3/24/2025 12:24:06 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoles](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](256) NULL,
	[NormalizedName] [nvarchar](256) NULL,
	[ConcurrencyStamp] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetRoles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserClaims]    Script Date: 3/24/2025 12:24:06 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserLogins]    Script Date: 3/24/2025 12:24:06 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserLogins](
	[LoginProvider] [nvarchar](450) NOT NULL,
	[ProviderKey] [nvarchar](450) NOT NULL,
	[ProviderDisplayName] [nvarchar](max) NULL,
	[UserId] [int] NOT NULL,
 CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY CLUSTERED 
(
	[LoginProvider] ASC,
	[ProviderKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserRoles]    Script Date: 3/24/2025 12:24:06 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserRoles](
	[UserId] [int] NOT NULL,
	[RoleId] [int] NOT NULL,
 CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUsers]    Script Date: 3/24/2025 12:24:06 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUsers](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FirstName] [nvarchar](max) NOT NULL,
	[LastName] [nvarchar](max) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[DeletedAt] [datetime2](7) NULL,
	[UserName] [nvarchar](256) NULL,
	[NormalizedUserName] [nvarchar](256) NULL,
	[Email] [nvarchar](256) NULL,
	[NormalizedEmail] [nvarchar](256) NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[PasswordHash] [nvarchar](max) NULL,
	[SecurityStamp] [nvarchar](max) NULL,
	[ConcurrencyStamp] [nvarchar](max) NULL,
	[PhoneNumber] [nvarchar](max) NULL,
	[PhoneNumberConfirmed] [bit] NOT NULL,
	[TwoFactorEnabled] [bit] NOT NULL,
	[LockoutEnd] [datetimeoffset](7) NULL,
	[LockoutEnabled] [bit] NOT NULL,
	[AccessFailedCount] [int] NOT NULL,
 CONSTRAINT [PK_AspNetUsers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserTokens]    Script Date: 3/24/2025 12:24:06 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserTokens](
	[UserId] [int] NOT NULL,
	[LoginProvider] [nvarchar](450) NOT NULL,
	[Name] [nvarchar](450) NOT NULL,
	[Value] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[LoginProvider] ASC,
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CartItems]    Script Date: 3/24/2025 12:24:06 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CartItems](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Quantity] [int] NOT NULL,
	[CartId] [int] NOT NULL,
	[ProductId] [int] NOT NULL,
	[DateAdded] [datetime2](7) NOT NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_CartItems] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Carts]    Script Date: 3/24/2025 12:24:06 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Carts](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_Carts] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Categories]    Script Date: 3/24/2025 12:24:06 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Categories](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CategoryName] [nvarchar](max) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_Categories] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrderLines]    Script Date: 3/24/2025 12:24:06 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderLines](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Quantity] [int] NOT NULL,
	[Price] [decimal](18, 2) NOT NULL,
	[OrderId] [int] NOT NULL,
	[ProductId] [int] NOT NULL,
	[ProductName] [nvarchar](max) NULL,
	[ProductImage] [nvarchar](max) NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_OrderLines] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Orders]    Script Date: 3/24/2025 12:24:06 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Orders](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrderNumber] [nvarchar](max) NOT NULL,
	[Total] [decimal](18, 2) NOT NULL,
	[OrderState] [int] NOT NULL,
	[OrderDate] [datetime2](7) NOT NULL,
	[UserName] [nvarchar](max) NOT NULL,
	[AddressId] [int] NULL,
	[AddressTitle] [nvarchar](max) NOT NULL,
	[AddressText] [nvarchar](max) NOT NULL,
	[City] [nvarchar](max) NOT NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_Orders] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Products]    Script Date: 3/24/2025 12:24:06 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Products](
	[ProductId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[Image] [nvarchar](max) NOT NULL,
	[Stock] [int] NOT NULL,
	[Price] [decimal](18, 2) NOT NULL,
	[IsHome] [bit] NOT NULL,
	[IsApproved] [bit] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CategoryId] [int] NOT NULL,
 CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED 
(
	[ProductId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250323202300_initial', N'6.0.36')
GO
SET IDENTITY_INSERT [dbo].[Addresses] ON 

INSERT [dbo].[Addresses] ([Id], [Title], [FullAddress], [City], [UserId], [IsDefault], [IsActive]) VALUES (1, N'Adres 1', N'Adres 1 adresi sokak', N'Adresimin Şehri', 2, 1, 1)
SET IDENTITY_INSERT [dbo].[Addresses] OFF
GO
SET IDENTITY_INSERT [dbo].[AspNetRoles] ON 

INSERT [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (1, N'Admin', N'ADMIN', N'f8654424-4db3-4b8e-8b3e-6f4c98d5afc9')
INSERT [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (2, N'User', N'USER', N'ee02dea5-4f2e-4de5-84ee-90be4dbc777b')
SET IDENTITY_INSERT [dbo].[AspNetRoles] OFF
GO
INSERT [dbo].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (1, 1)
INSERT [dbo].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (2, 2)
INSERT [dbo].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (3, 2)
INSERT [dbo].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (4, 2)
INSERT [dbo].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (5, 2)
GO
SET IDENTITY_INSERT [dbo].[AspNetUsers] ON 

INSERT [dbo].[AspNetUsers] ([Id], [FirstName], [LastName], [IsActive], [DeletedAt], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (1, N'Admin', N'User', 1, NULL, N'admin', N'ADMIN', N'admin@example.com', N'ADMIN@EXAMPLE.COM', 0, N'AQAAAAEAACcQAAAAEHIqkVZuaHose/8/8zqLINRu5ItB6QjeIW1KCW2yd/ORksklwe6yrdAFgTxSJ/uzvQ==', N'GMNCBM3FZ4UNOAL6S6ZLNSFE2KCXCLIE', N'9246a18a-699c-4ccf-b80b-45a6423dc3b3', NULL, 0, 0, NULL, 1, 0)
INSERT [dbo].[AspNetUsers] ([Id], [FirstName], [LastName], [IsActive], [DeletedAt], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (2, N'Normal', N'User', 1, NULL, N'normal', N'NORMAL', N'normal@example.com', N'NORMAL@EXAMPLE.COM', 0, N'AQAAAAEAACcQAAAAEF1zaBcSFs/jaVINvjSFtaWhcaeggWYUBLWbiAzSoqc5V3HdL/jNXI2ejDXGUqHecg==', N'YCQRUPY37UL3OPT3BKTI7H3QRJDZTBT4', N'8bd434cc-8f07-4bff-b897-a41cd65a401c', NULL, 0, 0, NULL, 1, 0)
INSERT [dbo].[AspNetUsers] ([Id], [FirstName], [LastName], [IsActive], [DeletedAt], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (3, N'Ahmet', N'Yılmaz', 1, NULL, N'ahmetyilmaz', N'AHMETYILMAZ', N'ahmet.yilmaz@example.com', N'AHMET.YILMAZ@EXAMPLE.COM', 1, N'AQAAAAEAACcQAAAAEHxRvTAVwGMUwz4GkGP7EUiQJh0YtGXnW0vh7SQG+HSobOT4WLzWJxFexZ7VuS8FgQ==', N'UCKPDS5R4XEOANLRWYEBI5YTLCUX35RD', N'c9609fee-b01a-46e5-a5c4-f33cb8340e1c', N'5551234567', 0, 0, NULL, 1, 0)
INSERT [dbo].[AspNetUsers] ([Id], [FirstName], [LastName], [IsActive], [DeletedAt], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (4, N'Ayşe', N'Kara', 1, NULL, N'aysekarauser', N'AYSEKARAUSER', N'ayse.kara@example.com', N'AYSE.KARA@EXAMPLE.COM', 1, N'AQAAAAEAACcQAAAAEJ/DTCAwOJ+9Q7H/l19KI38XMNewZOeO95qRh61D1vl/v1tgI9vXxZAhX7Ri133ytw==', N'DMXF6LPOFKYSOGVS5UWPPFCXWMVTRKL2', N'b8f90a05-be01-4b3f-8700-893123cdef01', N'5559876543', 0, 0, NULL, 1, 0)
INSERT [dbo].[AspNetUsers] ([Id], [FirstName], [LastName], [IsActive], [DeletedAt], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (5, N'Mehmet', N'Demir', 1, NULL, N'mehmetdemir', N'MEHMETDEMIR', N'mehmet.demir@example.com', N'MEHMET.DEMIR@EXAMPLE.COM', 1, N'AQAAAAEAACcQAAAAECL8fFqqGa4Bj3xpIu2BOWyMTIDktXyMiU9u/4OB+2MYYDFrD1vf+lkiGB/VkBZb7g==', N'RLPIC6H3YVPUDKRYCEFCERJ6QP42THOB', N'd5e123f7-a456-4b7c-8d90-123456abcdef', N'5553456789', 0, 0, NULL, 1, 0)
SET IDENTITY_INSERT [dbo].[AspNetUsers] OFF
GO
SET IDENTITY_INSERT [dbo].[Carts] ON 

INSERT [dbo].[Carts] ([Id], [UserId], [CreatedDate], [IsActive]) VALUES (1, 2, CAST(N'2025-03-24T00:03:21.4275738' AS DateTime2), 1)
SET IDENTITY_INSERT [dbo].[Carts] OFF
GO
SET IDENTITY_INSERT [dbo].[Categories] ON 

INSERT [dbo].[Categories] ([Id], [CategoryName], [Description], [IsActive]) VALUES (1, N'Elektronik', N'Elektronik ürünler ve teknolojik cihazlar', 1)
INSERT [dbo].[Categories] ([Id], [CategoryName], [Description], [IsActive]) VALUES (2, N'Giyim', N'Erkek, kadın ve çocuk giyim ürünleri', 1)
INSERT [dbo].[Categories] ([Id], [CategoryName], [Description], [IsActive]) VALUES (3, N'Ev & Yaşam', N'Ev dekorasyonu ve günlük yaşam ürünleri', 1)
INSERT [dbo].[Categories] ([Id], [CategoryName], [Description], [IsActive]) VALUES (4, N'Kitap & Kırtasiye', N'Kitaplar, dergiler ve kırtasiye malzemeleri', 1)
INSERT [dbo].[Categories] ([Id], [CategoryName], [Description], [IsActive]) VALUES (5, N'Spor & Outdoor', N'Spor ekipmanları ve outdoor ürünler', 1)
INSERT [dbo].[Categories] ([Id], [CategoryName], [Description], [IsActive]) VALUES (6, N'Kozmetik & Kişisel Bakım', N'Güzellik ve kişisel bakım ürünleri', 1)
SET IDENTITY_INSERT [dbo].[Categories] OFF
GO
SET IDENTITY_INSERT [dbo].[OrderLines] ON 

INSERT [dbo].[OrderLines] ([Id], [Quantity], [Price], [OrderId], [ProductId], [ProductName], [ProductImage], [IsActive]) VALUES (1, 1, CAST(24999.99 AS Decimal(18, 2)), 1, 1, N'Samsung Galaxy S24', N'/images/products/phone1.jpg', 1)
SET IDENTITY_INSERT [dbo].[OrderLines] OFF
GO
SET IDENTITY_INSERT [dbo].[Orders] ON 

INSERT [dbo].[Orders] ([Id], [OrderNumber], [Total], [OrderState], [OrderDate], [UserName], [AddressId], [AddressTitle], [AddressText], [City], [IsActive]) VALUES (1, N'e68bd2e12378427383e4aceda06646bd', CAST(24999.99 AS Decimal(18, 2)), 0, CAST(N'2025-03-24T00:04:21.8427292' AS DateTime2), N'normal', NULL, N'Adres 1', N'Adres 1 adresi sokak', N'Adresimin Şehri', 1)
SET IDENTITY_INSERT [dbo].[Orders] OFF
GO
SET IDENTITY_INSERT [dbo].[Products] ON 

INSERT [dbo].[Products] ([ProductId], [Name], [Description], [Image], [Stock], [Price], [IsHome], [IsApproved], [IsActive], [CategoryId]) VALUES (1, N'Samsung Galaxy S24', N'Samsung''ın en yeni akıllı telefon modeli 6.1 inç ekrana sahip', N'/images/products/phone1.jpg', 24, CAST(24999.99 AS Decimal(18, 2)), 1, 1, 1, 1)
INSERT [dbo].[Products] ([ProductId], [Name], [Description], [Image], [Stock], [Price], [IsHome], [IsApproved], [IsActive], [CategoryId]) VALUES (2, N'Apple MacBook Pro', N'13 inç M2 çipli MacBook Pro, 8GB RAM, 256GB SSD', N'/images/products/laptop1.jpg', 10, CAST(42999.99 AS Decimal(18, 2)), 1, 1, 1, 1)
INSERT [dbo].[Products] ([ProductId], [Name], [Description], [Image], [Stock], [Price], [IsHome], [IsApproved], [IsActive], [CategoryId]) VALUES (3, N'JBL Bluetooth Kulaklık', N'Kablosuz kulaklık, gürültü önleme teknolojisi', N'/images/products/headphone1.jpg', 50, CAST(1299.99 AS Decimal(18, 2)), 0, 1, 1, 1)
INSERT [dbo].[Products] ([ProductId], [Name], [Description], [Image], [Stock], [Price], [IsHome], [IsApproved], [IsActive], [CategoryId]) VALUES (4, N'Sony 4K Smart TV', N'55 inç Android TV, HDR desteği', N'/images/products/tv1.jpg', 15, CAST(17499.99 AS Decimal(18, 2)), 1, 1, 1, 1)
INSERT [dbo].[Products] ([ProductId], [Name], [Description], [Image], [Stock], [Price], [IsHome], [IsApproved], [IsActive], [CategoryId]) VALUES (5, N'Canon EOS M50', N'Aynasız fotoğraf makinesi, 24.1MP', N'/images/products/camera1.jpg', 8, CAST(13999.99 AS Decimal(18, 2)), 0, 1, 1, 1)
INSERT [dbo].[Products] ([ProductId], [Name], [Description], [Image], [Stock], [Price], [IsHome], [IsApproved], [IsActive], [CategoryId]) VALUES (6, N'Erkek Deri Ceket', N'Suni deri ceket, siyah renk, slim fit', N'/images/products/jacket1.jpg', 20, CAST(899.99 AS Decimal(18, 2)), 0, 1, 1, 2)
INSERT [dbo].[Products] ([ProductId], [Name], [Description], [Image], [Stock], [Price], [IsHome], [IsApproved], [IsActive], [CategoryId]) VALUES (7, N'Kadın Triko Kazak', N'Boğazlı triko kazak, krema rengi', N'/images/products/sweater1.jpg', 30, CAST(349.99 AS Decimal(18, 2)), 1, 1, 1, 2)
INSERT [dbo].[Products] ([ProductId], [Name], [Description], [Image], [Stock], [Price], [IsHome], [IsApproved], [IsActive], [CategoryId]) VALUES (8, N'Kot Pantolon', N'Yüksek bel, mavi, straight fit', N'/images/products/jeans1.jpg', 40, CAST(299.99 AS Decimal(18, 2)), 0, 1, 1, 2)
INSERT [dbo].[Products] ([ProductId], [Name], [Description], [Image], [Stock], [Price], [IsHome], [IsApproved], [IsActive], [CategoryId]) VALUES (9, N'Spor Ayakkabı', N'Günlük kullanım için rahat spor ayakkabı', N'/images/products/shoes1.jpg', 25, CAST(599.99 AS Decimal(18, 2)), 1, 1, 1, 2)
INSERT [dbo].[Products] ([ProductId], [Name], [Description], [Image], [Stock], [Price], [IsHome], [IsApproved], [IsActive], [CategoryId]) VALUES (10, N'Şal Desenli Elbise', N'Yazlık elbise, çiçek deseni, midi boy', N'/images/products/dress1.jpg', 15, CAST(449.99 AS Decimal(18, 2)), 0, 1, 1, 2)
INSERT [dbo].[Products] ([ProductId], [Name], [Description], [Image], [Stock], [Price], [IsHome], [IsApproved], [IsActive], [CategoryId]) VALUES (11, N'Modern Koltuk Takımı', N'3+2+1 oturma grubu, gri kumaş', N'/images/products/sofa1.jpg', 5, CAST(12999.99 AS Decimal(18, 2)), 1, 1, 1, 3)
INSERT [dbo].[Products] ([ProductId], [Name], [Description], [Image], [Stock], [Price], [IsHome], [IsApproved], [IsActive], [CategoryId]) VALUES (12, N'Yemek Masası Seti', N'6 kişilik ahşap masa ve sandalye seti', N'/images/products/table1.jpg', 8, CAST(5499.99 AS Decimal(18, 2)), 0, 1, 1, 3)
INSERT [dbo].[Products] ([ProductId], [Name], [Description], [Image], [Stock], [Price], [IsHome], [IsApproved], [IsActive], [CategoryId]) VALUES (13, N'Akıllı Robot Süpürge', N'Akıllı navigasyon, uzaktan kontrol', N'/images/products/vacuum1.jpg', 12, CAST(3499.99 AS Decimal(18, 2)), 1, 1, 1, 3)
INSERT [dbo].[Products] ([ProductId], [Name], [Description], [Image], [Stock], [Price], [IsHome], [IsApproved], [IsActive], [CategoryId]) VALUES (14, N'Yatak Örtüsü Takımı', N'Çift kişilik, pamuklu kumaş, mavi desen', N'/images/products/bedding1.jpg', 20, CAST(699.99 AS Decimal(18, 2)), 0, 1, 1, 3)
INSERT [dbo].[Products] ([ProductId], [Name], [Description], [Image], [Stock], [Price], [IsHome], [IsApproved], [IsActive], [CategoryId]) VALUES (15, N'LED Tavan Aydınlatma', N'Uzaktan kumandalı, renk değiştiren', N'/images/products/light1.jpg', 30, CAST(399.99 AS Decimal(18, 2)), 0, 1, 1, 3)
INSERT [dbo].[Products] ([ProductId], [Name], [Description], [Image], [Stock], [Price], [IsHome], [IsApproved], [IsActive], [CategoryId]) VALUES (16, N'Suç ve Ceza', N'Fyodor Dostoyevski, ciltli baskı', N'/images/products/book1.jpg', 50, CAST(89.99 AS Decimal(18, 2)), 0, 1, 1, 4)
INSERT [dbo].[Products] ([ProductId], [Name], [Description], [Image], [Stock], [Price], [IsHome], [IsApproved], [IsActive], [CategoryId]) VALUES (17, N'1984', N'George Orwell, karton kapak', N'/images/products/book2.jpg', 45, CAST(69.99 AS Decimal(18, 2)), 1, 1, 1, 4)
INSERT [dbo].[Products] ([ProductId], [Name], [Description], [Image], [Stock], [Price], [IsHome], [IsApproved], [IsActive], [CategoryId]) VALUES (18, N'Defter Seti', N'3''lü çizgili defter, A5 boyut', N'/images/products/notebook1.jpg', 100, CAST(49.99 AS Decimal(18, 2)), 0, 1, 1, 4)
INSERT [dbo].[Products] ([ProductId], [Name], [Description], [Image], [Stock], [Price], [IsHome], [IsApproved], [IsActive], [CategoryId]) VALUES (19, N'Kalem Seti', N'12 renkli kalem seti, su bazlı', N'/images/products/pens1.jpg', 80, CAST(34.99 AS Decimal(18, 2)), 0, 1, 1, 4)
INSERT [dbo].[Products] ([ProductId], [Name], [Description], [Image], [Stock], [Price], [IsHome], [IsApproved], [IsActive], [CategoryId]) VALUES (20, N'Akıllı Tablet Kalemi', N'Dijital çizim ve not alma kalemi', N'/images/products/stylus1.jpg', 25, CAST(399.99 AS Decimal(18, 2)), 1, 1, 1, 4)
INSERT [dbo].[Products] ([ProductId], [Name], [Description], [Image], [Stock], [Price], [IsHome], [IsApproved], [IsActive], [CategoryId]) VALUES (21, N'Koşu Bandı', N'Katlanabilir, 12 program', N'/images/products/treadmill1.jpg', 7, CAST(7999.99 AS Decimal(18, 2)), 1, 1, 1, 5)
INSERT [dbo].[Products] ([ProductId], [Name], [Description], [Image], [Stock], [Price], [IsHome], [IsApproved], [IsActive], [CategoryId]) VALUES (22, N'Yoga Matı', N'Kaymaz, 5mm kalınlık, mor renk', N'/images/products/yoga1.jpg', 40, CAST(149.99 AS Decimal(18, 2)), 0, 1, 1, 5)
INSERT [dbo].[Products] ([ProductId], [Name], [Description], [Image], [Stock], [Price], [IsHome], [IsApproved], [IsActive], [CategoryId]) VALUES (23, N'Dumbbell Set', N'2x5kg + 2x10kg ağırlık seti', N'/images/products/weights1.jpg', 15, CAST(899.99 AS Decimal(18, 2)), 0, 1, 1, 5)
INSERT [dbo].[Products] ([ProductId], [Name], [Description], [Image], [Stock], [Price], [IsHome], [IsApproved], [IsActive], [CategoryId]) VALUES (24, N'Kamp Çadırı', N'4 kişilik, su geçirmez', N'/images/products/tent1.jpg', 10, CAST(1499.99 AS Decimal(18, 2)), 1, 1, 1, 5)
INSERT [dbo].[Products] ([ProductId], [Name], [Description], [Image], [Stock], [Price], [IsHome], [IsApproved], [IsActive], [CategoryId]) VALUES (25, N'Trekking Ayakkabısı', N'Su geçirmez, erkek, kahverengi', N'/images/products/boots1.jpg', 20, CAST(799.99 AS Decimal(18, 2)), 0, 1, 1, 5)
INSERT [dbo].[Products] ([ProductId], [Name], [Description], [Image], [Stock], [Price], [IsHome], [IsApproved], [IsActive], [CategoryId]) VALUES (26, N'Parfüm', N'Unisex, odunsu ve baharatlı notalar', N'/images/products/perfume1.jpg', 30, CAST(699.99 AS Decimal(18, 2)), 1, 1, 1, 6)
INSERT [dbo].[Products] ([ProductId], [Name], [Description], [Image], [Stock], [Price], [IsHome], [IsApproved], [IsActive], [CategoryId]) VALUES (27, N'Cilt Bakım Seti', N'Temizleyici, tonik ve nemlendirici içerir', N'/images/products/skincare1.jpg', 25, CAST(349.99 AS Decimal(18, 2)), 0, 1, 1, 6)
INSERT [dbo].[Products] ([ProductId], [Name], [Description], [Image], [Stock], [Price], [IsHome], [IsApproved], [IsActive], [CategoryId]) VALUES (28, N'Saç Düzleştirici', N'Seramik kaplama, ısı ayarlı', N'/images/products/hair1.jpg', 18, CAST(499.99 AS Decimal(18, 2)), 0, 1, 1, 6)
INSERT [dbo].[Products] ([ProductId], [Name], [Description], [Image], [Stock], [Price], [IsHome], [IsApproved], [IsActive], [CategoryId]) VALUES (29, N'Makyaj Paleti', N'18 renk göz farı', N'/images/products/makeup1.jpg', 35, CAST(279.99 AS Decimal(18, 2)), 1, 1, 1, 6)
INSERT [dbo].[Products] ([ProductId], [Name], [Description], [Image], [Stock], [Price], [IsHome], [IsApproved], [IsActive], [CategoryId]) VALUES (30, N'Elektrikli Diş Fırçası', N'Şarj edilebilir, 3 fırça başlıklı', N'/images/products/toothbrush1.jpg', 22, CAST(449.99 AS Decimal(18, 2)), 0, 1, 1, 6)
SET IDENTITY_INSERT [dbo].[Products] OFF
GO
/****** Object:  Index [IX_Addresses_UserId]    Script Date: 3/24/2025 12:24:06 AM ******/
CREATE NONCLUSTERED INDEX [IX_Addresses_UserId] ON [dbo].[Addresses]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_AspNetRoleClaims_RoleId]    Script Date: 3/24/2025 12:24:06 AM ******/
CREATE NONCLUSTERED INDEX [IX_AspNetRoleClaims_RoleId] ON [dbo].[AspNetRoleClaims]
(
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [RoleNameIndex]    Script Date: 3/24/2025 12:24:06 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [RoleNameIndex] ON [dbo].[AspNetRoles]
(
	[NormalizedName] ASC
)
WHERE ([NormalizedName] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_AspNetUserClaims_UserId]    Script Date: 3/24/2025 12:24:06 AM ******/
CREATE NONCLUSTERED INDEX [IX_AspNetUserClaims_UserId] ON [dbo].[AspNetUserClaims]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_AspNetUserLogins_UserId]    Script Date: 3/24/2025 12:24:06 AM ******/
CREATE NONCLUSTERED INDEX [IX_AspNetUserLogins_UserId] ON [dbo].[AspNetUserLogins]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_AspNetUserRoles_RoleId]    Script Date: 3/24/2025 12:24:06 AM ******/
CREATE NONCLUSTERED INDEX [IX_AspNetUserRoles_RoleId] ON [dbo].[AspNetUserRoles]
(
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [EmailIndex]    Script Date: 3/24/2025 12:24:06 AM ******/
CREATE NONCLUSTERED INDEX [EmailIndex] ON [dbo].[AspNetUsers]
(
	[NormalizedEmail] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UserNameIndex]    Script Date: 3/24/2025 12:24:06 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex] ON [dbo].[AspNetUsers]
(
	[NormalizedUserName] ASC
)
WHERE ([NormalizedUserName] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CartItems_CartId]    Script Date: 3/24/2025 12:24:06 AM ******/
CREATE NONCLUSTERED INDEX [IX_CartItems_CartId] ON [dbo].[CartItems]
(
	[CartId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CartItems_ProductId]    Script Date: 3/24/2025 12:24:06 AM ******/
CREATE NONCLUSTERED INDEX [IX_CartItems_ProductId] ON [dbo].[CartItems]
(
	[ProductId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Carts_UserId]    Script Date: 3/24/2025 12:24:06 AM ******/
CREATE NONCLUSTERED INDEX [IX_Carts_UserId] ON [dbo].[Carts]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_OrderLines_OrderId]    Script Date: 3/24/2025 12:24:06 AM ******/
CREATE NONCLUSTERED INDEX [IX_OrderLines_OrderId] ON [dbo].[OrderLines]
(
	[OrderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_OrderLines_ProductId]    Script Date: 3/24/2025 12:24:06 AM ******/
CREATE NONCLUSTERED INDEX [IX_OrderLines_ProductId] ON [dbo].[OrderLines]
(
	[ProductId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Orders_AddressId]    Script Date: 3/24/2025 12:24:06 AM ******/
CREATE NONCLUSTERED INDEX [IX_Orders_AddressId] ON [dbo].[Orders]
(
	[AddressId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Products_CategoryId]    Script Date: 3/24/2025 12:24:06 AM ******/
CREATE NONCLUSTERED INDEX [IX_Products_CategoryId] ON [dbo].[Products]
(
	[CategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Addresses]  WITH CHECK ADD  CONSTRAINT [FK_Addresses_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Addresses] CHECK CONSTRAINT [FK_Addresses_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetRoleClaims]  WITH CHECK ADD  CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetRoleClaims] CHECK CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserClaims]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserClaims] CHECK CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserLogins]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserLogins] CHECK CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserTokens]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserTokens] CHECK CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[CartItems]  WITH CHECK ADD  CONSTRAINT [FK_CartItems_Carts_CartId] FOREIGN KEY([CartId])
REFERENCES [dbo].[Carts] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CartItems] CHECK CONSTRAINT [FK_CartItems_Carts_CartId]
GO
ALTER TABLE [dbo].[CartItems]  WITH CHECK ADD  CONSTRAINT [FK_CartItems_Products_ProductId] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Products] ([ProductId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CartItems] CHECK CONSTRAINT [FK_CartItems_Products_ProductId]
GO
ALTER TABLE [dbo].[Carts]  WITH CHECK ADD  CONSTRAINT [FK_Carts_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Carts] CHECK CONSTRAINT [FK_Carts_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[OrderLines]  WITH CHECK ADD  CONSTRAINT [FK_OrderLines_Orders_OrderId] FOREIGN KEY([OrderId])
REFERENCES [dbo].[Orders] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[OrderLines] CHECK CONSTRAINT [FK_OrderLines_Orders_OrderId]
GO
ALTER TABLE [dbo].[OrderLines]  WITH CHECK ADD  CONSTRAINT [FK_OrderLines_Products_ProductId] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Products] ([ProductId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[OrderLines] CHECK CONSTRAINT [FK_OrderLines_Products_ProductId]
GO
ALTER TABLE [dbo].[Orders]  WITH CHECK ADD  CONSTRAINT [FK_Orders_Addresses_AddressId] FOREIGN KEY([AddressId])
REFERENCES [dbo].[Addresses] ([Id])
GO
ALTER TABLE [dbo].[Orders] CHECK CONSTRAINT [FK_Orders_Addresses_AddressId]
GO
ALTER TABLE [dbo].[Products]  WITH CHECK ADD  CONSTRAINT [FK_Products_Categories_CategoryId] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Categories] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Products] CHECK CONSTRAINT [FK_Products_Categories_CategoryId]
GO
USE [master]
GO
ALTER DATABASE [ETicaret] SET  READ_WRITE 
GO
