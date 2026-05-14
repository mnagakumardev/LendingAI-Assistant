import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { loanApi } from '../api/loanApi';
import DocumentUpload from './DocumentUpload';
import '../styles/ApplicationDashboard.css';

interface Application {
  id: string;
  applicantInfo: {
    firstName: string;
    lastName: string;
    email: string;
    phoneNumber: string;
  };
  loanDetails: {
    loanAmount: number;
    loanTermMonths: number;
    loanType: string;
    annualIncome: number;
  };
  status: string;
  createdAt: string;
}

const ApplicationDashboard: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [application, setApplication] = useState<Application | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<'overview' | 'documents' | 'assessment'>('overview');

  useEffect(() => {
    const fetchApplication = async () => {
      try {
        if (id) {
          const data = await loanApi.getApplication(id);
          setApplication(data);
        }
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to load application');
      } finally {
        setLoading(false);
      }
    };

    fetchApplication();
  }, [id]);

  const handleRunAssessment = async () => {
    if (!id) return;
    try {
      await loanApi.runAIAssessment(id);
      navigate(`/applications/${id}/assessment`);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to run assessment');
    }
  };

  const handleSubmit = async () => {
    if (!id) return;
    try {
      await loanApi.submitApplication(id);
      if (application) {
        setApplication({ ...application, status: 'SUBMITTED' });
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to submit application');
    }
  };

  if (loading) return <div className="loading">Loading application...</div>;
  if (error) return <div className="error">{error}</div>;
  if (!application) return <div className="error">Application not found</div>;

  return (
    <div className="dashboard-container">
      <div className="dashboard-header">
        <h2>Loan Application #{id?.substring(0, 8)}</h2>
        <span className={`status-badge status-${application.status.toLowerCase()}`}>
          {application.status}
        </span>
      </div>

      <div className="tabs">
        <button 
          className={`tab ${activeTab === 'overview' ? 'active' : ''}`}
          onClick={() => setActiveTab('overview')}
        >
          Overview
        </button>
        <button 
          className={`tab ${activeTab === 'documents' ? 'active' : ''}`}
          onClick={() => setActiveTab('documents')}
        >
          Documents
        </button>
        <button 
          className={`tab ${activeTab === 'assessment' ? 'active' : ''}`}
          onClick={() => setActiveTab('assessment')}
        >
          Assessment
        </button>
      </div>

      <div className="tab-content">
        {activeTab === 'overview' && (
          <div className="overview-section">
            <div className="info-grid">
              <div className="info-card">
                <h3>Applicant Information</h3>
                <p><strong>Name:</strong> {application.applicantInfo.firstName} {application.applicantInfo.lastName}</p>
                <p><strong>Email:</strong> {application.applicantInfo.email}</p>
                <p><strong>Phone:</strong> {application.applicantInfo.phoneNumber}</p>
              </div>

              <div className="info-card">
                <h3>Loan Details</h3>
                <p><strong>Amount:</strong> ${application.loanDetails.loanAmount.toLocaleString()}</p>
                <p><strong>Term:</strong> {application.loanDetails.loanTermMonths} months</p>
                <p><strong>Type:</strong> {application.loanDetails.loanType}</p>
                <p><strong>Annual Income:</strong> ${application.loanDetails.annualIncome.toLocaleString()}</p>
              </div>
            </div>

            <div className="action-buttons">
              <button className="btn-primary" onClick={handleRunAssessment}>
                Run AI Assessment
              </button>
              <button className="btn-secondary" onClick={handleSubmit} disabled={application.status === 'SUBMITTED'}>
                Submit Application
              </button>
            </div>
          </div>
        )}

        {activeTab === 'documents' && <DocumentUpload applicationId={id!} />}

        {activeTab === 'assessment' && (
          <div className="assessment-section">
            <p>Run an AI assessment to see the analysis.</p>
            <button className="btn-primary" onClick={handleRunAssessment}>
              Start Assessment
            </button>
          </div>
        )}
      </div>
    </div>
  );
};

export default ApplicationDashboard;