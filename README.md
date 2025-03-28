# TodoListApp
## ✨ Features

This To-Do List web application includes the following key functionalities:

- 🔐 **User Authentication**  
  Basic login and registration system to manage personal task lists.

- ✅ **Task Management (CRUD)**  
  Create, edit, and delete your daily to-do tasks with ease.

- 🔍 **Smart Search**  
  Search tasks by **Name**, **Start Date**, or **End Date** using dynamic input switching.

- 🎯 **Advanced Filtering**  
  Filter tasks based on:
  - **Category**
  - **Due Date** → *Today, Past, or Future*
  - **Status** → *Pending, Completed, etc.*

---

## ⚙️ Technologies Used

| Category        | Technology                                |
|----------------|--------------------------------------------|
| 👨‍💻 Platform       | Visual Studio 2022                       |
| 🧠 Language       | C#, JavaScript, HTML, CSS                |
| 🧱 Framework      | ASP.NET Core MVC                         |
| 🗃️ Database       | SQL Server (LocalDB) via Entity Framework Core |

---

## 🚀 How to Run the App
**1. Open Visual Studio 2022**

Launch Visual Studio and select "Clone a repository".
Paste the repository URL and choose your local path.

**2. Set Up the Database**

Open the **Package Manager Console** and run:
<pre> Update-Database </pre>
![image](https://github.com/user-attachments/assets/28ae96cc-b833-4583-9e96-58a3efa95d9b)

* This will apply the existing migrations and create the database.

**3. Build and Run**

Press F5 or click the Run (IIS Express) button to launch the application.

**4. Login or Register**

On the landing page, create a new user or log in using an existing account.
