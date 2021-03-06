USE [master]

ALTER DATABASE [PowerShellWorkflowExtendedStore] SET COMPATIBILITY_LEVEL = 100
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [PowerShellWorkflowExtendedStore].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [PowerShellWorkflowExtendedStore] SET ANSI_NULL_DEFAULT OFF
GO
ALTER DATABASE [PowerShellWorkflowExtendedStore] SET ANSI_NULLS OFF
GO
ALTER DATABASE [PowerShellWorkflowExtendedStore] SET ANSI_PADDING OFF
GO
ALTER DATABASE [PowerShellWorkflowExtendedStore] SET ANSI_WARNINGS OFF
GO
ALTER DATABASE [PowerShellWorkflowExtendedStore] SET ARITHABORT OFF
GO
ALTER DATABASE [PowerShellWorkflowExtendedStore] SET AUTO_CLOSE OFF
GO
ALTER DATABASE [PowerShellWorkflowExtendedStore] SET AUTO_CREATE_STATISTICS ON
GO
ALTER DATABASE [PowerShellWorkflowExtendedStore] SET AUTO_SHRINK OFF
GO
ALTER DATABASE [PowerShellWorkflowExtendedStore] SET AUTO_UPDATE_STATISTICS ON
GO
ALTER DATABASE [PowerShellWorkflowExtendedStore] SET CURSOR_CLOSE_ON_COMMIT OFF
GO
ALTER DATABASE [PowerShellWorkflowExtendedStore] SET CURSOR_DEFAULT  GLOBAL
GO
ALTER DATABASE [PowerShellWorkflowExtendedStore] SET CONCAT_NULL_YIELDS_NULL OFF
GO
ALTER DATABASE [PowerShellWorkflowExtendedStore] SET NUMERIC_ROUNDABORT OFF
GO
ALTER DATABASE [PowerShellWorkflowExtendedStore] SET QUOTED_IDENTIFIER OFF
GO
ALTER DATABASE [PowerShellWorkflowExtendedStore] SET RECURSIVE_TRIGGERS OFF
GO
ALTER DATABASE [PowerShellWorkflowExtendedStore] SET  DISABLE_BROKER
GO
ALTER DATABASE [PowerShellWorkflowExtendedStore] SET AUTO_UPDATE_STATISTICS_ASYNC OFF
GO
ALTER DATABASE [PowerShellWorkflowExtendedStore] SET DATE_CORRELATION_OPTIMIZATION OFF
GO
ALTER DATABASE [PowerShellWorkflowExtendedStore] SET TRUSTWORTHY OFF
GO
ALTER DATABASE [PowerShellWorkflowExtendedStore] SET ALLOW_SNAPSHOT_ISOLATION OFF
GO
ALTER DATABASE [PowerShellWorkflowExtendedStore] SET PARAMETERIZATION SIMPLE
GO
ALTER DATABASE [PowerShellWorkflowExtendedStore] SET READ_COMMITTED_SNAPSHOT OFF
GO
ALTER DATABASE [PowerShellWorkflowExtendedStore] SET HONOR_BROKER_PRIORITY OFF
GO
ALTER DATABASE [PowerShellWorkflowExtendedStore] SET  READ_WRITE
GO
ALTER DATABASE [PowerShellWorkflowExtendedStore] SET RECOVERY FULL
GO
ALTER DATABASE [PowerShellWorkflowExtendedStore] SET  MULTI_USER
GO
ALTER DATABASE [PowerShellWorkflowExtendedStore] SET PAGE_VERIFY CHECKSUM
GO
ALTER DATABASE [PowerShellWorkflowExtendedStore] SET DB_CHAINING OFF
GO
EXEC sys.sp_db_vardecimal_storage_format N'PowerShellWorkflowExtendedStore', N'ON'
GO
USE [PowerShellWorkflowExtendedStore]
GO
/****** Object:  Table [dbo].[WorkflowState]    Script Date: 09/08/2011 17:39:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[WorkflowState](
	[WorkflowInstanceID] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Data] [varbinary](max) NULL,
 CONSTRAINT [PK_WorkflowState] PRIMARY KEY CLUSTERED 
(
	[WorkflowInstanceID] ASC,
	[Name] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
USE [master]
GO