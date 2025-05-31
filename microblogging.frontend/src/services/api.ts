// services/api.ts
import axios from 'axios';

const api = axios.create({
  baseURL: process.env.REACT_APP_API_URL,
});

// Add a request interceptor to attach the token automatically
api.interceptors.request.use(
  config => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  error => Promise.reject(error)
);

// Auth and posts APIs
export const login = (username: string, password: string) =>
  api.post('/auth/login', null, {
    params: { username, password },
  });

export const createPost = (formData: FormData) =>
  api.post('/post', formData);

export const getTimeline = () =>
  api.get('/post/timeline');
