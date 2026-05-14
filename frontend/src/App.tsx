import React, { useState } from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import LoanApplicationForm from './components/LoanApplicationForm';
import ApplicationDashboard from './components/ApplicationDashboard';
import AIAssessment from './components/AIAssessment';
import './App.css';

function App() {
  return (
    <Router>
      <div className="app">
        <header className="app-header">
          <h1>🏦 LendingAI Assistant</h1>
          <p>AI-Powered Loan Application Platform</p>
        </header>
        <main className="app-main">
          <Routes>
            <Route path="/" element={<LoanApplicationForm />} />
            <Route path="/applications/:id" element={<ApplicationDashboard />} />
            <Route path="/applications/:id/assessment" element={<AIAssessment />} />
          </Routes>
        </main>
      </div>
    </Router>
  );
}

export default App;