# Skill Extraction Tool

A full-stack web application that automatically extracts professional skills from CVs using AI-powered analysis. Built with ASP.NET Core Web API backend, React frontend, DuckDB for data persistence, and OpenAI GPT-4 for intelligent skill extraction.

## 🚀 Features

- **AI-Powered Skill Extraction**: Utilizes ChatGPT (GPT-4o-mini) to accurately analyze and extract skills from CVs
- **Multi-Format Support**: Upload CVs in PDF, PNG, JPG, or JPEG formats
- **User Authentication**: Secure JWT-based authentication system
- **Upload History**: All processed CVs and extracted data saved to user profiles
- **Download Original Files**: Retrieve uploaded CV files anytime
- **Responsive Design**: Modern, mobile-friendly interface
- **Real-time Processing**: Live upload progress and instant results

## 📋 Architecture

```
Repository Structure:
├── BackEnd/              # ASP.NET Core Web API
│   └── SkillExtractionApi/
│       ├── Controllers/  # API endpoints
│       ├── Services/     # Business logic
│       ├── Models/       # Data models
│       ├── Data/         # DuckDB context
│       └── DTOs/         # Data transfer objects
├── FrontEnd/             # React SPA
│   └── src/
│       ├── components/   # Reusable components
│       ├── contexts/     # React contexts
│       ├── pages/        # Page components
│       └── services/     # API integration
└── ConsoleSpike/         # Original proof of concept
```

## 🛠️ Technologies

### Backend
- **ASP.NET Core 9.0** - Web API framework
- **DuckDB** - Embedded analytical database
- **OpenAI API** (v2.1.0) - AI skill extraction
- **PDFtoImage** (v5.0.0) - PDF processing
- **JWT Bearer Auth** - Secure authentication
- **BCrypt.Net** - Password hashing

### Frontend
- **React 18** - UI library
- **React Router** - Client-side routing
- **Axios** - HTTP client
- **Vite** - Build tool and dev server

## ⚙️ Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (v18 or higher) with npm
- OpenAI API key ([Get one here](https://platform.openai.com/api-keys))

## 🚀 Getting Started

### 1. Clone the Repository

```powershell
git clone <repository-url>
cd Splike
```

### 2. Backend Setup

```powershell
cd BackEnd/SkillExtractionApi

# Restore NuGet packages
dotnet restore

# Update appsettings.Local.json with your configuration
Copy-Item appsettings.json appsettings.Local.json

# Edit appsettings.Local.json:
# - Add your OpenAI API key
# - Generate a strong JWT secret (min 32 characters)
```

**appsettings.Local.json** example:
```json
{
  "OpenAI": {
    "ApiKey": "sk-proj-YOUR-ACTUAL-OPENAI-API-KEY-HERE"
  },
  "JWT": {
    "Secret": "your-super-secret-jwt-key-minimum-32-characters-long"
  }
}
```

```powershell
# Run the backend
dotnet run
```

Backend will start at `https://localhost:7199` (and `http://localhost:5000`)

### 3. Frontend Setup

```powershell
cd ../../FrontEnd

# Install dependencies
npm install

# Start development server
npm run dev
```

Frontend will start at `http://localhost:3000`

## 📖 API Documentation

API documentation is available via Swagger UI when running the backend in development mode:
- Navigate to `https://localhost:7199/swagger`

### Key Endpoints

**Authentication**
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login and get JWT token
- `GET /api/auth/me` - Get current user info

**CV Management**
- `POST /api/cv/upload` - Upload and process CV (requires auth)
- `GET /api/cv/history` - Get user's upload history (requires auth)
- `GET /api/cv/{id}` - Get specific CV details (requires auth)
- `GET /api/cv/{id}/download` - Download original CV file (requires auth)
- `DELETE /api/cv/{id}` - Delete CV (requires auth)

## 🎯 Usage Flow

1. **Register/Login**: Create an account or login with existing credentials
2. **Upload CV**: Navigate to Upload page and select/drag-drop your CV file
3. **Processing**: AI analyzes the CV and extracts skills (takes 5-15 seconds)
4. **Review Results**: View extracted skills and professional summary
5. **Profile Management**: Access all uploaded CVs from your profile page
6. **Download/Delete**: Manage your CV history as needed

## 🔐 Security

- Passwords hashed with BCrypt (cost factor: 11)
- JWT tokens for stateless authentication (24-hour expiration)
- CORS configured for allowed origins only
- API keys stored in local configuration (not in version control)
- File upload validation (type and size limits)
- User data isolation (users can only access their own data)

## 💾 Data Storage

- **DuckDB**: Stores user accounts and CV metadata
- **File System**: Stores uploaded CV files in `./uploads/cvs/`
- **Database File**: `skillextraction.db` (created automatically)

## 🎨 Frontend Features

### Pages
1. **Landing Page**: Public homepage with features overview
2. **Login/Register**: Authentication forms
3. **Upload Page**: CV upload interface with drag-and-drop
4. **Profile Page**: View, download, and delete uploaded CVs

### Components
- **PrivateRoute**: Protected route wrapper for authenticated pages
- **AuthContext**: Global authentication state management
- **API Service**: Centralized API communication layer

## 🔧 Configuration

### Backend Configuration (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DuckDB": "Data Source=skillextraction.db"
  },
  "OpenAI": {
    "ApiKey": "your-openai-api-key-here"
  },
  "JWT": {
    "Secret": "your-jwt-secret-key-minimum-32-characters-long-here",
    "Issuer": "SkillExtractionApi",
    "Audience": "SkillExtractionClient",
    "ExpirationMinutes": 1440
  },
  "FileStorage": {
    "CvStoragePath": "./uploads/cvs"
  },
  "CORS": {
    "AllowedOrigins": ["http://localhost:3000", "http://localhost:5173"]
  }
}
```

### Frontend Configuration (vite.config.js)

```javascript
export default defineConfig({
  server: {
    port: 3000,
    proxy: {
      '/api': {
        target: 'https://localhost:7199',
        changeOrigin: true,
        secure: false
      }
    }
  }
})
```

## 📦 Build for Production

### Backend
```powershell
cd BackEnd/SkillExtractionApi
dotnet publish -c Release -o ./publish
```

### Frontend
```powershell
cd FrontEnd
npm run build
# Output in /dist directory
```

## 🐛 Troubleshooting

### Backend Issues

**Certificate errors (HTTPS)**:
```powershell
dotnet dev-certs https --trust
```

**Database connection errors**:
- Ensure DuckDB.NET.Data package is installed
- Check file permissions for database directory

**OpenAI API errors**:
- Verify API key is valid and has credits
- Check network connectivity
- Review rate limits

### Frontend Issues

**CORS errors**:
- Ensure backend CORS configuration includes frontend URL
- Check backend is running

**Authentication failures**:
- Clear localStorage: `localStorage.clear()`
- Check JWT token expiration settings

## 📝 Development Notes

### Database Schema

**Users Table**:
- Id (INTEGER, PRIMARY KEY)
- Username (VARCHAR, UNIQUE)
- Email (VARCHAR, UNIQUE)
- PasswordHash (VARCHAR)
- CreatedAt (TIMESTAMP)

**CvUploads Table**:
- Id (INTEGER, PRIMARY KEY)
- UserId (INTEGER, FOREIGN KEY)
- FileName (VARCHAR)
- FilePath (VARCHAR)
- UploadDate (TIMESTAMP)
- FileSize (BIGINT)
- ExtractedSkills (VARCHAR/JSON)
- OpenAiResponse (VARCHAR/JSON)
- ProcessingStatus (VARCHAR)

### Suggested Improvements

1. **Rate Limiting**: Implement per-user upload limits to control OpenAI API costs
2. **Real-time Progress**: Add SignalR for live CV processing updates
3. **Skill Categorization**: Enhance AI prompt to categorize skills by type
4. **Search & Filters**: Add search functionality for uploaded CVs
5. **Email Notifications**: Send email when CV processing completes
6. **Multi-language Support**: Detect and handle CVs in different languages

## 📄 License

This project is for educational/demonstration purposes.

## 👥 Contributing

This is a demonstration project. Feel free to fork and extend it for your own use.

## 🙏 Acknowledgments

- OpenAI for GPT-4o-mini API
- DuckDB for the embedded database
- PDFtoImage library for PDF processing
- React and ASP.NET Core communities

---

**Built with ❤️ using ASP.NET Core, React, and AI**
