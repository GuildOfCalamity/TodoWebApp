## 💻 TodoWebApp - ASP.NET

![Icon](./Documentation/AppIcon.png)

## 📝 v1.0.0.0 - June 2025

**Dependencies**

| Assembly | Version |
| ---- | ---- |
| NET Core | 9.0.6 |
| ASP.NET Core | 9.0.6 |
| Microsoft.Entity Framework | 9.0.6 |
| Microsoft.EntityFrameworkCore.Design | 9.0.6 |
| Microsoft.EntityFrameworkCore.InMemory | 9.0.6 |
| Microsoft.EntityFrameworkCore.SqlServer | 9.0.6 |

## 📰 Description
- A simple **ToDo List** web application utilizing **ASP.NET Core**, **Bootstrap** and **SQL/EntityFramework**.

## 🎛️ First Run

- First setup the database by running the following commands in the Package Manager Console:

```bash
PM> dotnet tool install --global dotnet-ef
PM> dotnet ef migrations add InitialCreate
PM> dotnet ef database update
```

- Then launch the application using **VisualStudio** or the following command:

	- `C:\> TodoWebApp --port=5050`

	- `PS> .\TodoWebApp --port=5050`

- If no port is specified, then the application will use the routes specified in the `appsettings.json` file.

- I've provided additional details in the `/Documentation` folder.

## 📷 Screenshot

![Sample](./Documentation/Screenshot3.png)

## 🧾 License/Warranty
* Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish and distribute copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions: The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
* The software is provided "as is", without warranty of any kind, express or implied, including but not limited to the warranties of merchantability, fitness for a particular purpose and noninfringement. In no event shall the author or copyright holder be liable for any claim, damages or other liability, whether in an action of contract, tort or otherwise, arising from, out of or in connection with the software or the use or other dealings in the software.
* Copyright © 2025. All rights reserved.

## 📋 Proofing
* This application was compiled and tested using *VisualStudio* 2022 on *Windows 10/11* versions **22H2**, **21H2**, **21H1**, and **23H2**.

