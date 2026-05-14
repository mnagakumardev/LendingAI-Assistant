import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { loanApi } from '../api/loanApi';
import '../styles/LoanApplicationForm.css';

interface FormData {
  applicantName: string;
  email: string;
  phoneNumber: string;
  loanAmount: number;
  loanTermMonths: number;
  loanType: string;
  purpose: string;
  annualIncome: number;
}

const LoanApplicationForm: React.FC = () => {
  const navigate = useNavigate();
  const [formData, setFormData] = useState<FormData>({
    applicantName: '',
    email: '',
    phoneNumber: '',
    loanAmount: 0,
    loanTermMonths: 60,
    loanType: 'Personal',
    purpose: '',
    annualIncome: 0,
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: name.includes('Amount') || name.includes('Income') || name.includes('Months') 
        ? parseFloat(value) 
        : value
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      const response = await loanApi.createApplication(formData);
      navigate(`/applications/${response.id}`);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An error occurred');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="form-container">
      <div className="form-card">
        <h2>Loan Application Form</h2>
        {error && <div className="error-message">{error}</div>}
        
        <form onSubmit={handleSubmit}>
          <div className="form-section">
            <h3>Personal Information</h3>
            
            <div className="form-group">
              <label htmlFor="applicantName">Full Name *</label>
              <input
                type="text"
                id="applicantName"
                name="applicantName"
                value={formData.applicantName}
                onChange={handleChange}
                required
                placeholder="John Doe"
              />
            </div>

            <div className="form-row">
              <div className="form-group">
                <label htmlFor="email">Email *</label>
                <input
                  type="email"
                  id="email"
                  name="email"
                  value={formData.email}
                  onChange={handleChange}
                  required
                  placeholder="john@example.com"
                />
              </div>

              <div className="form-group">
                <label htmlFor="phoneNumber">Phone Number *</label>
                <input
                  type="tel"
                  id="phoneNumber"
                  name="phoneNumber"
                  value={formData.phoneNumber}
                  onChange={handleChange}
                  required
                  placeholder="(555) 123-4567"
                />
              </div>
            </div>
          </div>

          <div className="form-section">
            <h3>Loan Details</h3>
            
            <div className="form-row">
              <div className="form-group">
                <label htmlFor="loanAmount">Loan Amount ($) *</label>
                <input
                  type="number"
                  id="loanAmount"
                  name="loanAmount"
                  value={formData.loanAmount}
                  onChange={handleChange}
                  required
                  min="1000"
                  max="1000000"
                  placeholder="50000"
                />
              </div>

              <div className="form-group">
                <label htmlFor="loanTermMonths">Loan Term (Months) *</label>
                <input
                  type="number"
                  id="loanTermMonths"
                  name="loanTermMonths"
                  value={formData.loanTermMonths}
                  onChange={handleChange}
                  required
                  min="12"
                  max="360"
                />
              </div>
            </div>

            <div className="form-row">
              <div className="form-group">
                <label htmlFor="loanType">Loan Type *</label>
                <select
                  id="loanType"
                  name="loanType"
                  value={formData.loanType}
                  onChange={handleChange}
                  required
                >
                  <option value="Personal">Personal Loan</option>
                  <option value="Home">Home Loan</option>
                  <option value="Auto">Auto Loan</option>
                  <option value="Business">Business Loan</option>
                </select>
              </div>

              <div className="form-group">
                <label htmlFor="purpose">Purpose *</label>
                <input
                  type="text"
                  id="purpose"
                  name="purpose"
                  value={formData.purpose}
                  onChange={handleChange}
                  required
                  placeholder="Debt consolidation, Home improvement, etc."
                />
              </div>
            </div>

            <div className="form-group">
              <label htmlFor="annualIncome">Annual Income ($) *</label>
              <input
                type="number"
                id="annualIncome"
                name="annualIncome"
                value={formData.annualIncome}
                onChange={handleChange}
                required
                min="0"
                placeholder="75000"
              />
            </div>
          </div>

          <button type="submit" className="submit-button" disabled={loading}>
            {loading ? 'Creating Application...' : 'Create Loan Application'}
          </button>
        </form>
      </div>
    </div>
  );
};

export default LoanApplicationForm;