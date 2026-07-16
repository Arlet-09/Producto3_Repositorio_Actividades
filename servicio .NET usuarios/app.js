const url = "http://localhost:5069/usuario";

fetch(url)
  .then(res => res.json())
  .then(data => {
      console.log(data);
  })
  .catch(err => {
      console.error("Error:", err);
  });

