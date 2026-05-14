import React, { useState } from 'react';
import { loanApi } from '../api/loanApi';
import '../styles/DocumentUpload.css';

interface DocumentUploadProps {
  applicationId: string;
}

const DocumentUpload: React.FC<DocumentUploadProps> = ({ applicationId }) => {
  const [files, setFiles] = useState<File[]>([]);
  const [uploading, setUploading] = useState(false);
  const [message, setMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files) {
      setFiles(Array.from(e.target.files));
    }
  };

  const handleUpload = async () => {
    if (files.length === 0) return;

    setUploading(true);
    try {
      const formData = new FormData();
      files.forEach(file => formData.append('files', file));

      await loanApi.processDocuments(applicationId, formData);
      setMessage({ type: 'success', text: 'Documents uploaded successfully!' });
      setFiles([]);
    } catch (err) {
      setMessage({ type: 'error', text: err instanceof Error ? err.message : 'Upload failed' });
    } finally {
      setUploading(false);
    }
  };

  return (
    <div className="document-upload">
      <h3>Upload Supporting Documents</h3>
      {message && (
        <div className={`message message-${message.type}`}>
          {message.text}
        </div>
      )}

      <div className="upload-area">
        <input
          type="file"
          id="file-input"
          multiple
          onChange={handleFileSelect}
          disabled={uploading}
        />
        <label htmlFor="file-input" className="upload-label">
          <div className="upload-icon">📄</div>
          <p>Drag files here or click to browse</p>
          <p className="upload-hint">Accepted: PDF, Images, Documents</p>
        </label>
      </div>

      {files.length > 0 && (
        <div className="file-list">
          <h4>Selected Files:</h4>
          <ul>
            {files.map((file, idx) => (
              <li key={idx}>{file.name}</li>
            ))}
          </ul>
          <button 
            className="btn-primary"
            onClick={handleUpload}
            disabled={uploading}
          >
            {uploading ? 'Uploading...' : 'Upload Documents'}
          </button>
        </div>
      )}
    </div>
  );
};

export default DocumentUpload;