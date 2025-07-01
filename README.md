## 💻 TodoWebApp - ASP.NET

![Icon](./Documentation/AppIcon.png)

## 📝 v1.0.0.0 - July 2025

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
- A simple **ToDo List** web application utilizing [ASP.NET Core](https://dotnet.microsoft.com/en-us/apps/aspnet), [Bootstrap](https://getbootstrap.com/docs/5.3/getting-started/introduction) and [Entity Framework](https://learn.microsoft.com/en-us/aspnet/entity-framework).
- Basic features include validation techniques for the data entry, a warning icon that appears when an item is past due, the ability to sort by date added or due date, ability to search items, sticky toolbar, item details tooltip on hover, and custom Bootstrap/CSS properties.
- The **Entity Framework** in this project explores the basic **CRUD** model.
    - `Index.cshtml` - Displays the list of **ToDo** items, with sorting.
	- `Create.cshtml` - Allows the user to add a new **ToDo** item.
	- `Edit.cshtml` - Allows the user to edit an existing **ToDo** item.
	- `Delete.cshtml` - Allows the user to delete an existing **ToDo** item.

## 🎛️ First Run

- First setup the database by running the following commands in the **Package Manager Console**:

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

## 📷 Screenshots

![Main](./Documentation/Screenshot3.png)

![Search](./Documentation/Screenshot4.png)

![Tooltip](./Documentation/Screenshot5.png)

![Edit](./Documentation/Screenshot6.png)

## 🧾 License/Warranty
* Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish and distribute copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions: The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
* The software is provided "as is", without warranty of any kind, express or implied, including but not limited to the warranties of merchantability, fitness for a particular purpose and noninfringement. In no event shall the author or copyright holder be liable for any claim, damages or other liability, whether in an action of contract, tort or otherwise, arising from, out of or in connection with the software or the use or other dealings in the software.
* Copyright © 2025. All rights reserved.

## 📋 Proofing
* This application was compiled and tested using *VisualStudio* 2022 on *Windows 10/11* versions **22H2**, **21H2**, **21H1**, and **23H2**.

