import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { getCvHistory, deleteCv, downloadCv } from '../services/api';
import './ProfilePage.css';

const ProfilePage = () => {
  const [uploads, setUploads] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [selectedCv, setSelectedCv] = useState(null);
  const [deleteConfirm, setDeleteConfirm] = useState(null);

  const { user, logout } = useAuth();

  useEffect(() => {
    loadHistory();
  }, []);

  const loadHistory = async () => {
    try {
      setLoading(true);
      const data = await getCvHistory();
      setUploads(data.uploads || []);
    } catch (err) {
      setError('Failed to load CV history');
    } finally {
      setLoading(false);
    }
  };

  const handleDownload = async (id, fileName) => {
    try {
      await downloadCv(id, fileName);
    } catch (err) {
      setError('Failed to download CV');
    }
  };

  const handleDelete = async (id) => {
    try {
      await deleteCv(id);
      setUploads(uploads.filter(cv => cv.id !== id));
      setDeleteConfirm(null);
      setSelectedCv(null);
    } catch (err) {
      setError('Failed to delete CV');
    }
  };

  const formatDate = (dateString) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const formatFileSize = (bytes) => {
    return (bytes / 1024 / 1024).toFixed(2) + ' MB';
  };

  return (
    <div className="profile-page">
      <nav className="navbar">
        <div className="container">
          <h1 className="logo">Skill Extraction Tool</h1>
          <div className="nav-links">
            <span className="user-info">Welcome, {user?.username}</span>
            <Link to="/upload" className="btn btn-primary">Upload New CV</Link>
            <button onClick={logout} className="btn btn-secondary">Logout</button>
          </div>
        </div>
      </nav>

      <main className="profile-main">
        <div className="profile-container">
          <div className="profile-header">
            <div>
              <h2>Your CV Uploads</h2>
              <p className="subtitle">Manage and review your uploaded CVs and extracted skills</p>
            </div>
            <div className="stats">
              <div className="stat-card">
                <span className="stat-number">{uploads.length}</span>
                <span className="stat-label">Total Uploads</span>
              </div>
            </div>
          </div>

          {error && <div className="error-message">{error}</div>}

          {loading ? (
            <div className="loading">Loading your uploads...</div>
          ) : uploads.length === 0 ? (
            <div className="empty-state">
              <p className="empty-icon">📄</p>
              <h3>No CVs uploaded yet</h3>
              <p>Get started by uploading your first CV</p>
              <Link to="/upload" className="btn btn-primary">Upload CV</Link>
            </div>
          ) : (
            <div className="uploads-grid">
              {uploads.map((cv) => (
                <div key={cv.id} className="cv-card">
                  <div className="cv-card-header">
                    <h3>{cv.fileName}</h3>
                    <span className={`status-badge ${cv.processingStatus.toLowerCase()}`}>
                      {cv.processingStatus}
                    </span>
                  </div>
                  
                  <div className="cv-card-info">
                    <p><strong>Uploaded:</strong> {formatDate(cv.uploadDate)}</p>
                    <p><strong>Size:</strong> {formatFileSize(cv.fileSize)}</p>
                    {cv.extractedSkills && cv.extractedSkills.length > 0 && (
                      <p><strong>Skills Found:</strong> {cv.extractedSkills.length}</p>
                    )}
                  </div>

                  {cv.processingStatus === 'Completed' && cv.extractedSkills && cv.extractedSkills.length > 0 && (
                    <div className="skills-preview">
                      {cv.extractedSkills.slice(0, 3).map((skill, index) => (
                        <span key={index} className="skill-tag-mini">{skill}</span>
                      ))}
                      {cv.extractedSkills.length > 3 && (
                        <span className="more-skills">+{cv.extractedSkills.length - 3} more</span>
                      )}
                    </div>
                  )}

                  <div className="cv-card-actions">
                    <button onClick={() => setSelectedCv(cv)} className="btn btn-secondary btn-small">
                      View Details
                    </button>
                    <button onClick={() => handleDownload(cv.id, cv.fileName)} className="btn btn-secondary btn-small">
                      Download
                    </button>
                    <button onClick={() => setDeleteConfirm(cv.id)} className="btn btn-danger btn-small">
                      Delete
                    </button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </main>

      {/* Details Modal */}
      {selectedCv && (
        <div className="modal-overlay" onClick={() => setSelectedCv(null)}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h3>{selectedCv.fileName}</h3>
              <button className="modal-close" onClick={() => setSelectedCv(null)}>×</button>
            </div>
            
            <div className="modal-body">
              <div className="detail-section">
                <h4>Summary</h4>
                <p className="summary-text">{selectedCv.summary || 'No summary available'}</p>
              </div>

              <div className="detail-section">
                <h4>Extracted Skills ({selectedCv.extractedSkills?.length || 0})</h4>
                <div className="skills-list-modal">
                  {selectedCv.extractedSkills && selectedCv.extractedSkills.length > 0 ? (
                    selectedCv.extractedSkills.map((skill, index) => (
                      <span key={index} className="skill-tag">{skill}</span>
                    ))
                  ) : (
                    <p>No skills extracted</p>
                  )}
                </div>
              </div>

              <div className="detail-section">
                <h4>File Information</h4>
                <p><strong>Upload Date:</strong> {formatDate(selectedCv.uploadDate)}</p>
                <p><strong>File Size:</strong> {formatFileSize(selectedCv.fileSize)}</p>
                <p><strong>Status:</strong> {selectedCv.processingStatus}</p>
              </div>
            </div>

            <div className="modal-actions">
              <button onClick={() => handleDownload(selectedCv.id, selectedCv.fileName)} className="btn btn-primary">
                Download CV
              </button>
              <button onClick={() => { setDeleteConfirm(selectedCv.id); setSelectedCv(null); }} className="btn btn-danger">
                Delete
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Delete Confirmation Modal */}
      {deleteConfirm && (
        <div className="modal-overlay" onClick={() => setDeleteConfirm(null)}>
          <div className="modal-content modal-small" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h3>Confirm Deletion</h3>
              <button className="modal-close" onClick={() => setDeleteConfirm(null)}>×</button>
            </div>
            
            <div className="modal-body">
              <p>Are you sure you want to delete this CV? This action cannot be undone.</p>
            </div>

            <div className="modal-actions">
              <button onClick={() => setDeleteConfirm(null)} className="btn btn-secondary">
                Cancel
              </button>
              <button onClick={() => handleDelete(deleteConfirm)} className="btn btn-danger">
                Delete
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default ProfilePage;
