# Microblogging Platform

A simple microblogging application built with **.NET 8**, **React**, **MongoDB**, and **Docker**. Users can post text and upload images. Uploaded images are processed and resized in the background to fit multiple device screen sizes.

![Microblogging Demo](./abjaad-microblogging.gif)

---

## How to Run with Docker Compose

From the root of the **backend folder**, run:

```bash
docker compose up --build
```

> Ensure Docker is installed and running on your machine.

### Access the system via:

- Frontend (React app): [http://localhost:3000](http://localhost:3000)
- Backend API (Swagger UI): [http://localhost:8080/index.html](http://localhost:8080/index.html)
- Image assets: Accessible via `http://localhost:8080/uploads/<image-name>.webp`

---

##  Project Structure

```
microblogging.solution/
├── Microblogging.API          # .NET 8 Web API
├── Microblogging.Service      # Business logic & background processing
├── Microblogging.Repository   # MongoDB repository layer
├── Microblogging.Domain       # Domain entities
├── Microblogging.Shared       # Shared DTOs, responses
├── uploads/                   # Uploaded images (original & resized)
└── ../microblogging.frontend  # React + TypeScript frontend
```

---

##  Image Upload Design

### Backend Upload & Resize Flow

- **Original image** is uploaded immediately to allow quick frontend display.
- The **original** is stored and its URL returned in the response.
- A **background queue** handles multi-size image generation (400w, 800w, 1200w) using a strategy pattern to make the system extensible for other storage providers.
- Resized images are served dynamically based on the screen size using the `<picture>` HTML element in the frontend.

###  Design Patterns Used

- **Strategy Pattern** for storage providers (e.g., local, Azure Blob, S3 support).
- **Queue-Based Background Service** using hosted service in .NET for async image resizing.
- **Repository Pattern** for MongoDB operations.

---

##  Frontend Work (React)

- Built with **React + TypeScript**
- Responsive design with support for displaying images at different sizes.
- Optimized image rendering using `<picture>` and `media` queries.
- Shows a loader for images until they are ready.
- Uses `.env` configuration for backend URLs to adapt for local/Docker usage.
- Handles image post delays gracefully to ensure display without refresh.

---

##  Backend Work (.NET 8)

- API built using **ASP.NET Core 8**
- MongoDB used for storing posts
- File uploads with `IFormFile`
- Local storage (via mounted Docker volume)
- Background image resizing service
- Environment support for development via Docker Compose
- Swagger for API documentation

---

## Task Completion Summary

- [x] Post tweet with image upload
- [x] Show tweet with responsive images
- [x] Store original image immediately for instant access
- [x] Background resize into 3 variants (400w, 800w, 1200w)
- [x] Strategy pattern for pluggable storage (local/Azure-ready)
- [x] Picture element for dynamic image rendering in frontend
- [x] Loader handling while image is processing
- [x] Prevent image flashing or missing due to async resize delay
- [x] Cache busting with timestamps
- [x] Docker Compose setup for full system
- [x] Fully working demo GIF to showcase features

---

## Environment Variables

Set these in `.env` (for development):

```
REACT_APP_API_BASE_URL=http://localhost:8080/api
REACT_APP_BASE_URL=http://localhost:8080
```

These values are injected during `docker compose up`.

---
