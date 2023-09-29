const prod = {
  url: {
    API_URL: window.location.origin + ':5001',
  }
};
const dev = {
  url: {
    API_URL: ''
  }
};

export const config = process.env.NODE_ENV === 'development' ? dev : prod;
