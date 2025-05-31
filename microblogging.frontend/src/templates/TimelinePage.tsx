import React, { useEffect, useState } from 'react';
import { createPost, getTimeline } from '../services/api';

const backendUrl = process.env.REACT_APP_API_BASE_URL || 'http://localhost:8080';

export default function TimelinePage() {
  const [text, setText] = useState('');
  const [image, setImage] = useState<File | null>(null);
  const [posts, setPosts] = useState<any[]>([]);
  const [newlyAddedPostKeys, setNewlyAddedPostKeys] = useState<string[]>([]);
  const [loadedImages, setLoadedImages] = useState<Record<string, boolean>>({});
  const [imageLoaded, setImageLoaded] = useState<Record<string, boolean>>({});
  const [lastPostedAt, setLastPostedAt] = useState<number | null>(null);


  const token = localStorage.getItem('token') || '';

  useEffect(() => {
    const timer = setTimeout(() => {
      fetchTimeline();
    }, 500);

    return () => clearTimeout(timer);
  }, []);

  

  const fetchTimeline = async (isNewPost = false) => {
  try {
    const response = await getTimeline();
    const data = response.data.data;
    setPosts(data);

    if (isNewPost && data.length > 0) {
      const newKey = `${data[0].createdAt}_${data[0].username}`;
      waitForImage(`${backendUrl}${data[0].imageVariants?.['400w']}`, () => {
        setImageLoaded(prev => ({ ...prev, [newKey]: true }));
      });
    }
  } catch (error) {
    console.error('Failed to fetch timeline:', error);
  }
};

const waitForImage = (url: string, onReady: () => void, timeout = 8000) => {
  const start = Date.now();
  const check = () => {
    const img = new Image();
    img.onload = onReady;
    img.onerror = () => {
      if (Date.now() - start < timeout) {
        setTimeout(check, 400);
      }
    };
    img.src = url + `?cb=${Date.now()}`; // avoid caching
  };
  check();
};

  const handleSubmit = async (e: React.FormEvent) => {
  e.preventDefault();
  const formData = new FormData();
  formData.append('Text', text);
  if (image) formData.append('Image', image);

  try {
    await createPost(formData);
    setText('');
    setImage(null);

    
    setTimeout(() => {
      fetchTimeline(true); 
    }, 500);

    
  } catch (error) {
    console.error('Post creation failed:', error);
  }

};


  return (
    <div className="timeline">
      <form onSubmit={handleSubmit} className="post-form">
        <textarea
          value={text}
          onChange={e => setText(e.target.value)}
          placeholder="What's on your mind?"
          maxLength={140}
          required
        />
        <input
          type="file"
          accept="image/*"
          onChange={e => setImage(e.target.files?.[0] || null)}
        />
        <button type="submit">Post</button>
      </form>

      <div className="post-list">
        {posts.map(post => {
           const createdAtTime = new Date(post.createdAt).getTime();

  
  if (lastPostedAt && createdAtTime >= lastPostedAt && Date.now() - lastPostedAt < 500) {
    return null;
  }

  const cacheBuster = `?ts=${new Date(post.createdAt).getTime()}`;
  const postKey = `${post.createdAt}_${post.username}`;
  const isLoaded = imageLoaded[postKey];

  return (
    <div key={postKey} className="post">
      <p><strong>@{post.username}</strong></p>
      <p>{post.text}</p>

      {post.originalImageUrl && (
        <div key={`${postKey}_${isLoaded ? 'loaded' : 'loading'}`}>
          {!isLoaded && (
            <div style={{
              width: '100%',
              height: '220px',
              background: '#f3f3f3',
              display: 'flex',
              justifyContent: 'center',
              alignItems: 'center'
            }}>
              <div className="spinner" />
            </div>
          )}

          <picture style={isLoaded ? {} : { display: 'none' }}>
            {post.imageVariants?.['400w'] && (
              <source media="(max-width: 480px)" srcSet={`${backendUrl}${post.imageVariants['400w']}${cacheBuster}`} />
            )}
            {post.imageVariants?.['800w'] && (
              <source media="(max-width: 768px)" srcSet={`${backendUrl}${post.imageVariants['800w']}${cacheBuster}`} />
            )}
            {post.imageVariants?.['1200w'] && (
              <source media="(min-width: 769px)" srcSet={`${backendUrl}${post.imageVariants['1200w']}${cacheBuster}`} />
            )}
            <img
              src={`${backendUrl}${post.originalImageUrl}${cacheBuster}`}
              alt="Post"
              loading="eager"
              onLoad={() =>
                setImageLoaded(prev => ({ ...prev, [postKey]: true }))
              }
              style={{ width: '100%', height: 'auto', borderRadius: '8px' }}
            />
          </picture>
        </div>
      )}
    </div>
  );
})}


      </div>
    </div>
  );
}
