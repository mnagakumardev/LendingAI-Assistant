import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { loanApi } from '../api/loanApi';
import '../styles/AIAssessment.css';

interface AssessmentResult {
  assessmentId: string;
  assessedAt: string;
  approvalProbability: number;
  recommendation: string;
  riskFactors: string[];
  positiveFactors: string[];
  analysisSummary: string;
  scoreBreakdown: Record<string, number>;
}

const AIAssessment: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [assessment, setAssessment] = useState<AssessmentResult | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchAssessment = async () => {
      try {
        if (id) {
          const data = await loanApi.getApplication(id);
          if (data.aiAssessment) {
            setAssessment(data.aiAssessment);
          } else {
            setError('No assessment available yet');
          }
        }
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to load assessment');
      } finally {
        setLoading(false);
      }
    };

    fetchAssessment();
  }, [id]);

  if (loading) return <div className="loading">Loading assessment...</div>;
  if (error) return <div className="error">{error}</div>;
  if (!assessment) return <div className="error">No assessment data found</div>;

  const getRecommendationColor = (recommendation: string) => {
    switch (recommendation) {
      case 'APPROVE': return 'success';
      case 'CONDITIONAL_APPROVAL': return 'warning';
      case 'DENY': return 'danger';
      default: return 'info';
    }
  };

  return (
    <div className="assessment-container">
      <div className="assessment-header">
        <h2>AI-Powered Loan Assessment</h2>
        <p>Assessment Date: {new Date(assessment.assessedAt).toLocaleDateString()}</p>
      </div>

      <div className="assessment-grid">
        <div className={`assessment-card recommendation-${getRecommendationColor(assessment.recommendation)}`}>
          <h3>Recommendation</h3>
          <div className="recommendation-value">{assessment.recommendation}</div>
          <div className="approval-probability">
            <span>Approval Probability:</span>
            <strong>{(assessment.approvalProbability * 100).toFixed(1)}%</strong>
          </div>
        </div>

        <div className="assessment-card">
          <h3>Analysis Summary</h3>
          <p>{assessment.analysisSummary}</p>
        </div>
      </div>

      <div className="assessment-details">
        <div className="detail-section">
          <h3>✅ Positive Factors</h3>
          <ul>
            {assessment.positiveFactors.map((factor, idx) => (
              <li key={idx}>{factor}</li>
            ))}
          </ul>
        </div>

        <div className="detail-section">
          <h3>⚠️ Risk Factors</h3>
          <ul>
            {assessment.riskFactors.map((factor, idx) => (
              <li key={idx}>{factor}</li>
            ))}
          </ul>
        </div>
      </div>

      {Object.keys(assessment.scoreBreakdown).length > 0 && (
        <div className="score-breakdown">
          <h3>Score Breakdown</h3>
          <div className="breakdown-chart">
            {Object.entries(assessment.scoreBreakdown).map(([key, value]) => (
              <div key={key} className="breakdown-item">
                <span className="breakdown-label">{key}</span>
                <div className="breakdown-bar">
                  <div 
                    className="breakdown-fill" 
                    style={{ width: `${Math.min(value * 10, 100)}%` }}
                  ></div>
                </div>
                <span className="breakdown-value">{value}</span>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
};

export default AIAssessment;