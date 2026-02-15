import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import './LandingPage.css';

const LandingPage = () => {
  const { isAuthenticated } = useAuth();
  const navigate = useNavigate();

  return (
    <div className="landing-page">
      <nav className="navbar">
        <div className="container">
          <h1 className="logo">Skill Extraction Tool</h1>
          <div className="nav-links">
            {isAuthenticated ? (
              <>
                <Link to="/upload" className="btn btn-primary">Upload CV</Link>
                <Link to="/profile" className="btn btn-secondary">Profile</Link>
              </>
            ) : (
              <>
                <Link to="/login" className="btn btn-secondary">Login</Link>
                <Link to="/register" className="btn btn-primary">Sign Up</Link>
              </>
            )}
          </div>
        </div>
      </nav>

      <section className="hero">
        <div className="container">
          <h2 className="hero-title">Extract Skills from CVs Automatically</h2>
          <p className="hero-subtitle">
            Upload your CV and let AI extract all your professional skills instantly
          </p>
          <div className="hero-actions">
            {isAuthenticated ? (
              <button onClick={() => navigate('/upload')} className="btn btn-large btn-primary">
                Upload Your CV
              </button>
            ) : (
              <>
                <Link to="/register" className="btn btn-large btn-primary">
                  Get Started
                </Link>
                <Link to="/login" className="btn btn-large btn-secondary">
                  Login
                </Link>
              </>
            )}
          </div>
        </div>
      </section>

      <section className="features">
        <div className="container">
          <h3 className="section-title">Key Features</h3>
          <div className="features-grid">
            <div className="feature-card">
              <h4>🤖 AI-Powered</h4>
              <p>Uses advanced ChatGPT AI to accurately extract skills from any CV format</p>
            </div>
            <div className="feature-card">
              <h4>📄 Multiple Formats</h4>
              <p>Support for PDF, PNG, JPG, and JPEG file formats</p>
            </div>
            <div className="feature-card">
              <h4>💾 Save History</h4>
              <p>All uploaded CVs and extracted skills are saved to your profile</p>
            </div>
            <div className="feature-card">
              <h4>⚡ Fast Processing</h4>
              <p>Get results in seconds with our optimized processing pipeline</p>
            </div>
          </div>
        </div>
      </section>

      <footer className="footer">
        <div className="container">
          <p>&copy; 2026 Skill Extraction Tool. All rights reserved.</p>
        </div>
      </footer>
    </div>
  );
};

export default LandingPage;
