const API_BASE_URL = 'http://localhost:5000/api';

export const loanApi = {
  async createApplication(data: any) {
    const response = await fetch(`${API_BASE_URL}/loanapplication/create`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    });
    if (!response.ok) throw new Error('Failed to create application');
    return response.json();
  },

  async getApplication(applicationId: string) {
    const response = await fetch(`${API_BASE_URL}/loanapplication/${applicationId}`);
    if (!response.ok) throw new Error('Failed to fetch application');
    return response.json();
  },

  async runAIAssessment(applicationId: string) {
    const response = await fetch(
      `${API_BASE_URL}/loanapplication/${applicationId}/ai-assessment`,
      { method: 'POST' }
    );
    if (!response.ok) throw new Error('Failed to run assessment');
    return response.json();
  },

  async processDocuments(applicationId: string, formData: FormData) {
    const response = await fetch(
      `${API_BASE_URL}/loanapplication/${applicationId}/documents`,
      { method: 'POST', body: formData }
    );
    if (!response.ok) throw new Error('Failed to process documents');
    return response.json();
  },

  async calculateCreditScore(applicationId: string) {
    const response = await fetch(
      `${API_BASE_URL}/loanapplication/${applicationId}/credit-score`,
      { method: 'POST' }
    );
    if (!response.ok) throw new Error('Failed to calculate credit score');
    return response.json();
  },

  async submitApplication(applicationId: string) {
    const response = await fetch(
      `${API_BASE_URL}/loanapplication/${applicationId}/submit`,
      { method: 'POST' }
    );
    if (!response.ok) throw new Error('Failed to submit application');
    return response.json();
  },
};