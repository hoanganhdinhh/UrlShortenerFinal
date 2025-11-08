# URL Shortener Project

This project is a URL Shortener built using .NET Core. It allows users to convert long URLs into shorter, more manageable links and redirects users to the original URL when accessed. The system follows modern DevOps practices and includes a fully automated CI/CD pipeline.

---

## Application Features

- [x] Generate a **unique short code** for a given URL.
- [x] **Redirect** users from the short link to the original URL.
- [x] **Validate input URLs** and handle errors gracefully with appropriate responses.
- [x] **Store shortened URLs and metadata** (e.g., creation time, click count) in a database.
- [] Provide a **RESTful API** for creating and retrieving shortened URLs.
- [] Include a **simple front-end web application** (Vue or React) for users to interact with the service.

---

## DevOps & Deployment Features

- [x] The application is **containerized** using a `Dockerfile`.
- [x] A **CI/CD pipeline** is implemented using **GitHub Actions**.
- [] The pipeline **automatically builds** a Docker image on every push to the `main` branch.
- [] After testing completes successfully, the pipeline **pushes the image** to a **public container registry** (e.g., Docker Hub) as a **versioned artifact**.
- [] The final step of the pipeline **automatically deploys** the application to a **PaaS platform** (e.g., Render) by triggering a **deploy hook**.

---

## Technologies Used

| Component     | Technology       |
|---------------|-----------------|
| Backend       | .NET Core Web API |
| Front-end     | Vue.js or React |
| Database      | MySQL |
| Containerization | Docker |
| CI/CD         | GitHub Actions |
| Deployment    | Render (or similar PaaS) |


