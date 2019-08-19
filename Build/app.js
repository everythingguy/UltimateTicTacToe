const express = require("express");

const app = express();

app.use(express.static("./public"));

app.get('/', (req, res) => {
    res.sendFile(__dirname + "/public/index.html");
});

const port = process.env.PORT || 80;
app.set('port', port);

app.listen(port, () => {
    console.log(`Web site started, listening on port ${port}`);
});