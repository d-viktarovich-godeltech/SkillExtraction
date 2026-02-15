import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { uploadCv } from '../services/api';
import './UploadPage.css';

const UploadPage = () => {
  const [file, setFile] = useState(null);
  const [dragActive, setDragActive] = useState(false);
  const [uploading, setUploading] = useState(false);
  const [uploadProgress, setUploadProgress] = useState(0);
  const [result, setResult] = useState(null);
  const [error, setError] = useState('');

  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleDrag = (e) => {
    e.preventDefault();
    e.stopPropagation();
    if (e.type === 'dragenter' || e.type === 'dragover') {
      setDragActive(true);
    } else if (e.type === 'dragleave') {
      setDragActive(false);
    }
  };

  const handleDrop = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setDragActive(false);

    if (e.dataTransfer.files && e.dataTransfer.files[0]) {
      handleFileSelect(e.dataTransfer.files[0]);
    }
  };

  const handleFileInput = (e) => {
    if (e.target.files && e.target.files[0]) {
      handleFileSelect(e.target.files[0]);
    }
  };

  const handleFileSelect = (selectedFile) => {
    const validTypes = ['application/pdf', 'image/png', 'image/jpeg', 'image/jpg'];
    if (!validTypes.includes(selectedFile.type)) {
      setError('Invalid file type. Please upload a PDF, PNG, JPG, or JPEG file.');
      return;
    }

    const maxSize = 10 * 1024 * 1024; // 10MB
    if (selectedFile.size > maxSize) {
      setError('File size exceeds 10MB limit.');
      return;
    }

    setFile(selectedFile);
    setError('');
    setResult(null);
  };

  const handleUpload = async () => {
    if (!file) return;

    setUploading(true);
    setError('');
    setResult(null);
    setUploadProgress(0);

    try {
      const data = await uploadCv(file, (progressEvent) => {
        const progress = Math.round((progressEvent.loaded * 100) / progressEvent.total);
        setUploadProgress(progress);
      });

      setResult(data);
      setFile(null);
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to upload CV. Please try again.');
    } finally {
      setUploading(false);
      setUploadProgress(0);
    }
  };

  const handleReset = () => {
    setFile(null);
    setResult(null);
    setError('');
  };

  return (
    <div className="upload-page">
      <nav className="navbar">
        <div className="container">
          <h1 className="logo">Skill Extraction Tool</h1>
          <div className="nav-links">
            <span className="user-info">Welcome, {user?.username}</span>
            <Link to="/profile" className="btn btn-secondary">Profile</Link>
            <button onClick={logout} className="btn btn-secondary">Logout</button>
          </div>
        </div>
      </nav>

      <main className="upload-main">
        <div className="upload-container">
          <h2>Upload Your CV</h2>
          <p className="subtitle">Supported formats: PDF, PNG, JPG, JPEG (Max 10MB)</p>

          {error && <div className="error-message">{error}</div>}

          {!result ? (
            <>
              <div
                className={`drop-zone ${dragActive ? 'active' : ''} ${file ? 'has-file' : ''}`}
                onDragEnter={handleDrag}
                onDragLeave={handleDrag}
                onDragOver={handleDrag}
                onDrop={handleDrop}
              >
                {file ? (
                  <div className="file-info">
                    <p className="file-icon">📄</p>
                    <p className="file-name">{file.name}</p>
                    <p className="file-size">{(file.size / 1024 / 1024).toFixed(2)} MB</p>
                    <button onClick={handleReset} className="btn-remove">Remove</button>
                  </div>
                ) : (
                  <>
                    <p className="drop-icon">📁</p>
                    <p>Drag and drop your CV here</p>
                    <p className="or-text">or</p>
                    <label htmlFor="file-input" className="btn btn-primary">
                      Browse Files
                    </label>
                    <input
                      id="file-input"
                      type="file"
                      accept=".pdf,.png,.jpg,.jpeg"
                      onChange={handleFileInput}
                      style={{ display: 'none' }}
                    />
                  </>
                )}
              </div>

              {file && (
                <button
                  onClick={handleUpload}
                  className="btn btn-primary btn-large"
                  disabled={uploading}
                >
                  {uploading ? `Uploading... ${uploadProgress}%` : 'Upload and Extract Skills'}
                </button>
              )}

              {uploading && (
                <div className="progress-bar">
                  <div className="progress-fill" style={{ width: `${uploadProgress}%` }}></div>
                </div>
              )}
            </>
          ) : (
            <div className="result-container">
              <div className="success-message">
                <h3>✓ CV Successfully Processed!</h3>
                <p>Your CV has been analyzed and skills have been extracted.</p>
              </div>

              <div className="result-section">
                <h4>Summary</h4>
                <p className="summary-text">{result.summary || 'No summary available'}</p>
              </div>

              <div className="result-section">
                <h4>Extracted Skills</h4>
                <div className="skills-list">
                  {result.extractedSkills && result.extractedSkills.length > 0 ? (
                    result.extractedSkills.map((skill, index) => (
                      <span key={index} className="skill-tag">{skill}</span>
                    ))
                  ) : (
                    <p>No skills extracted</p>
                  )}
                </div>
              </div>

              <div className="result-actions">
                <button onClick={handleReset} className="btn btn-primary">Upload Another CV</button>
                <button onClick={() => navigate('/profile')} className="btn btn-secondary">
                  View All Uploads
                </button>
              </div>
            </div>
          )}
        </div>
      </main>
    </div>
  );
};

export default UploadPage;
