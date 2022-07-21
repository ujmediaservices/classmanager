# Class Manager 

An API and application for managing a list of classes, class registrations, and associated lab environments. 

## Data Model 

### Organizations and Users

The top-level object is an organization, which represents a company, educational institution, or other group entity. Organizations can register themselves for manual approval along with a set of domain names their organization owns. Once approved, all registrations using an email associated with an organization's domain will be considered members of that organization. 

Organizations may be segmented into suborgs. A suborg may have classes that are exclusive to itself. Classes that should be organization-visible should be created at the organization level.

A top-level organization must have one administrator and can have up to three administrators. A suborg can optionally have up to five administrators. 

All users not registered with a third party organization are added to the ClassManager organization. These users can see all courses registered under ClassManager, which are considered publicly available. Users registered with a third party organization can see the public classes as well as all classes associated with their organization. 

### Courses and Classes

A course is a description of a learning offering. A class is an instance of a course occurring at a specified date and location.

## Authentication

In order to reduce user risk, Class Manager supports login via one-time passwords sent to email. It does not allow permanent passwords and does not store long-lived passwords in the database. 

The app uses JWT to manage and verify valid login sessions. Login sessions expire in 24 hours. If users selected the Remember Me option, sessions will expire after 14 days. 

Class Manager creates and manages its own user database and its own JWT tokens. This keeps user management independent of any cloud provider.

In the future, we will aim to support social media login as well as SSO via SAML.  

## Architecture

Class Manager is built on .NET Core. It exposes an API as well as a Web site.

The current version can spin up an infrastructure and deploy onto Azure. However, it is crafted so that you can deploy onto any cloud provider that supports containers. To deploy onto another cloud provider, you'll need to implement your own versions of the following classes:

* **gll.classmanager.hosting.SecureConfigurationProvider**: Fetches configuration values from a secure store, such as Azure Key Vault or AWS Secrets Manager.