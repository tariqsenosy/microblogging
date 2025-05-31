import React, { useEffect, useState } from 'react';
import { createPost, getTimeline } from '../services/api';

export default function TimelinePage() {
  const [text, setText] = useState('');
  const [image, setImage] = useState<File | null>(null);
  const [posts, setPosts] = useState<any[]>([]);
  const token = localStorage.getItem('token') || '';
  const backendUrl= process.env.REACT_APP_BASE_URL;

  useEffect(() => {
    getTimeline().then(res => setPosts(res.data.data));
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const formData = new FormData();
    formData.append('Text', text);
    if (image) formData.append('Image', image);

    await createPost(formData);
    setText('');
    setImage(null);
    const res = await getTimeline();
    setPosts(res.data.data);
  };

  return (
    <div className="timeline">
      <form onSubmit={handleSubmit} className="post-form">
        <textarea value={text} onChange={e => setText(e.target.value)} placeholder="What's on your mind?" maxLength={140} />
        <input type="file" accept="image/*" onChange={e => setImage(e.target.files?.[0] || null)} />
        <button type="submit">Post</button>
      </form>
      <div className="post-list">
        {posts.map((post, idx) => (
          <div key={post.id} className="post">
            <p><strong>@{post.username}</strong></p>
            <p>{post.text}</p>
           {post.originalImageUrl && (
  <picture>
    
    {post.imageVariants?.['400w'] && (
      <source media="(max-width: 480px)" srcSet={`${backendUrl}${post.imageVariants['400w']}`} />
    )}
    {post.imageVariants?.['800w'] && (
      <source media="(max-width: 768px)" srcSet={`${backendUrl}${post.imageVariants['800w']}`} />
    )}
    {post.imageVariants?.['1200w'] && (
      <source media="(min-width: 769px)" srcSet={`${backendUrl}${post.imageVariants['1200w']}`} />
    )}
    
    {/* Fallback to original */}
    <img
      src={`${backendUrl}${post.originalImageUrl}`}
      alt="Post"
      style={{ width: '100%', height: 'auto', borderRadius: '8px' }}
    />
  </picture>
)}

          </div>
        ))}
      </div>
    </div>
  );
}