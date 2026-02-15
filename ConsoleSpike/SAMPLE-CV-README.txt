SAMPLE CV INSTRUCTIONS
======================

To test this application, you need to provide a CV in one of these formats:
- PDF (.pdf)
- PNG (.png)
- JPEG (.jpg, .jpeg)

Options:
1. Place your CV as "sample-cv.pdf" (or .png, .jpg) in this directory and run: dotnet run
2. Run with a specific file: dotnet run "path/to/your/cv.pdf"

The application will:
- For PDF: Convert each page to an image
- For images (PNG/JPEG): Upload directly
- Send to ChatGPT using vision API for visual analysis

This preserves all formatting, layout, colors, and design elements!
