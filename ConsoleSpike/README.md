# CV Uploader - OpenAI ChatGPT Integration

A .NET 9 Console application that uploads CV documents directly to OpenAI's ChatGPT API using vision capabilities. Supports multiple formats: **PDF, PNG, JPEG**.

## Key Features

✅ **Direct File Upload** - No text extraction, preserves formatting and layout
✅ **Multi-Format Support** - PDF, PNG, JPG, JPEG
✅ **PDF Multi-Page** - Automatically converts all PDF pages to images
✅ **OpenAI Vision API** - Uses GPT-4o-mini with vision for accurate analysis

## Prerequisites

- .NET 9 SDK
- OpenAI API key
- A CV in PDF, PNG, or JPEG format

## Setup

1. **Get your OpenAI API key:**
   - Visit https://platform.openai.com/api-keys
   - Create a new API key

2. **Configure the application:**
   - **Option A (Recommended for local development):** Create `appsettings.Local.json`:
   ```json
   {
     "OpenAI": {
       "ApiKey": "sk-proj-your-actual-api-key"
     }
   }
   ```
   - **Option B:** Open `appsettings.json` and replace `your-api-key-here` with your actual key
   
   > Note: `appsettings.Local.json` is already in `.gitignore` and won't be committed to version control, making it safer for local development.

3. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

## Usage

### With command line argument:
```bash
dotnet run path/to/your/cv.pdf
dotnet run path/to/your/cv.png
dotnet run path/to/your/cv.jpg
```

### With default filename:
Place your CV as `sample-cv.pdf` (or .png, .jpg) in the project directory:
```bash
dotnet run
```

**Supported formats:** `.pdf`, `.png`, `.jpg`, `.jpeg`

## Features

- **Direct file upload** to ChatGPT without text extraction
- **Preserves visual formatting** - layout, fonts, colors all visible to AI
- **Multi-page PDF support** - automatically splits and processes all pages
- **Image support** - PNG and JPEG files work directly
- Receives AI-powered analysis and summary with extracted skills
- Secure API key management via configuration file
- Enhanced security: prevents command execution from CV content

## Project Structure

- `Program.cs` - Main application with direct file upload and PDF-to-image conversion
- `appsettings.json` - Configuration file for API key
- `sample-cv.*` - Place your CV file here (PDF, PNG, or JPG)
- `CVUploader.csproj` - Project file with dependencies

## Dependencies

- **OpenAI** (v2.1.0) - Official OpenAI .NET SDK with vision support
- **Microsoft.Extensions.Configuration** (v9.0.0) - Configuration management
- **Microsoft.Extensions.Configuration.Json** (v9.0.0) - JSON configuration support
- **PDFtoImage** (v4.2.0) - PDF to image conversion for multi-page support

## How It Works

1. **Image files** (PNG, JPEG): Uploaded directly to ChatGPT vision API
2. **PDF files**: Each page is converted to a PNG image, then all images are sent to ChatGPT
3. ChatGPT analyzes the visual content and provides feedback

This approach is superior to text extraction because:
- ✅ Preserves formatting and layout
- ✅ AI can see fonts, colors, and design choices
- ✅ Tables and graphics are visible
- ✅ Works with any CV design

## Security Note

⚠️ **Never commit your `appsettings.json` file with a real API key to version control!**

Consider adding `appsettings.json` to your `.gitignore` file and create an `appsettings.example.json` template instead.
