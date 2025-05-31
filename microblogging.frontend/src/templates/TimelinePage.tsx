import React, { useEffect, useState } from 'react';
import { createPost, getTimeline } from '../services/api';

export default function TimelinePage() {
  const [text, setText] = useState('');
  const [image, setImage] = useState<File | null>(null);
  const [posts, setPosts] = useState<any[]>([]);
  const token = localStorage.getItem('token') || '';

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
          <div key={idx} className="post">
            <p><strong>@{post.username}</strong></p>
            <p>{post.text}</p>
            {post.originalImageUrl && <img src={`https://localhost:44341${post.originalImageUrl}`} alt="Post" width={400} />}
          </div>
        ))}
      </div>
    </div>
  );
}