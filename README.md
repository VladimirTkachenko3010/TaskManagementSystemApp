<p align="center">
   <h1>Task Management System App</h1>
</p>
<p>This is a backend service for a simple task management system developed in .NET 8 using Entity Framework. The system allows users to register, log in, and manage their tasks. Each user has access only to their own tasks, and task filtering and sorting by priority, status, and due date are supported.</p>

<h2>Table of Contents</h2>
<ul>
    <li><a href="#features">Features</a></li>
    <li><a href="#technologies">Technologies</a></li>
    <li><a href="#setup">Setup</a></li>
    <li><a href="#api-endpoints">API Endpoints</a></li>
    <li><a href="#architecture">Architecture</a></li>
    <li><a href="#future-improvements">Future Improvements</a></li>
</ul>

<h2>Features</h2>
<ul>
    <li><strong>User Authentication:</strong> Secure registration and login using JWT (JSON Web Tokens).</li>
    <li><strong>Task Management:</strong> Users can create, update, delete, and retrieve tasks.</li>
    <li><strong>Filtering and Sorting:</strong> Tasks can be filtered and sorted by status, priority, and due date.</li>
    <li><strong>Pagination:</strong> Supports pagination when retrieving tasks.</li>
    <li><strong>Secure:</strong> All task-related endpoints are secured using JWT authentication.</li>
    <li><strong>Onion Architecture: </strong>The project structure follows the Onion Architecture pattern for better separation of concerns and easier maintainability.</li>
</ul>

<h2 id="technologies">Technologies</h2>
<ul>
    <li>.NET 8</li>
    <li>Entity Framework</li>
    <li>SQL Server (or other relational databases)</li>
    <li>JWT Authentication</li>
    <li>Dependency Injection</li>
    <li>Repository Pattern</li>
    <li>Logging</li>
</ul>

<h2 id="setup">Setup</h2>

<h3>Prerequisites</h3>
<ul>
    <li><a href="https://dotnet.microsoft.com/download/dotnet/8.0">.NET 8 SDK</a></li>
    <li><a href="https://www.microsoft.com/en-us/sql-server">SQL Server</a> or any other SQL database (e.g., PostgreSQL, MySQL)</li>
</ul>

<h3>Steps to Run Locally</h3>
<ol>
    <li>Clone the repository:
        <pre><code>git clone https://github.com/VladimirTkachenko3010/TaskManagementSystemApp
cd TaskManagementSystemApp</code></pre>
    </li>
    <li>Configure the database connection string in the <code>appsettings.json</code> file.</li>
    <li>Apply migrations to set up the database schema:
        <pre><code>dotnet ef database update</code></pre>
    </li>
    <li>Run the application:
        <pre><code>dotnet run</code></pre>
    </li>
</ol>

<h2 id="api-endpoints">API Endpoints</h2>

<h3>User Authentication</h3>
<ul>
    <li><strong>POST /users/register</strong>
        <p>Register a new user.</p>
        <pre><code>{
    "username": "user123",
    "email": "user@example.com",
    "password": "Password123!"
}</code></pre>
    </li>
    <li><strong>POST /users/login</strong>
        <p>Authenticate a user and return a JWT.</p>
        <pre><code>{
    "username": "user123",
    "password": "Password123!"
}</code></pre>
    </li>
</ul>

<h3>Task Management (JWT required)</h3>
<ul>
    <li><strong>POST /tasks</strong>
        <p>Create a new task.</p>
        <pre><code>{
    "title": "New Task",
    "description": "Task details",
    "dueDate": "2024-09-01T00:00:00",
    "status": "Pending",
    "priority": "High"
}</code></pre>
    </li>
    <li><strong>GET /tasks</strong>
        <p>Retrieve a list of tasks for the authenticated user. Supports filtering by <code>status</code>, <code>dueDate</code>, and <code>priority</code>, and pagination.</p>
    </li>
    <li><strong>GET /tasks/{id}</strong>
        <p>Retrieve details of a specific task.</p>
    </li>
    <li><strong>PUT /tasks/{id}</strong>
        <p>Update an existing task.</p>
    </li>
    <li><strong>DELETE /tasks/{id}</strong>
        <p>Delete a specific task.</p>
    </li>
</ul>

<h2 id="architecture">Architecture</h2>
<ul>
    <li><strong>Domain:</strong> Contains domain entities, enums. This is the core business logic layer of the application.</li>
    <li><strong>Application:</strong> Contains the service layer, which includes business logic and DTOs.</li>
    <li><strong>Infrastructure:</strong> Handles data access, external service integrations, and persistence. This layer interacts with the SQL database.</li>
    <li><strong>API:</strong> Exposes endpoints for user and task management. It handles HTTP requests and responses, and integrates with the service layer for business logic execution.</li>
    <li><strong>Test:</strong> This project includes unit tests using xUnit and Moq for mocking dependencies in service and repository layers.
    <p>To run the tests, execute:</p>
        <pre><code>{
    dotnet test
}</code></pre></li>
</ul>

<h2 id="future-improvements">Future Improvements</h2>
<ul>
    <li><strong>CI/CD:</strong> Implement GitHub Actions for continuous integration and deployment.</li>
    <li><strong>Unit Testing:</strong> Add unit tests for the service and repository layers.</li>
</ul>
