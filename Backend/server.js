const express = require('express');
const mongoose = require('mongoose');
const dotenv = require ('dotenv');

const{
    createPlayer,
    login,
    updateScore,
    getScore,
    deletePlayer,
    updatePassword
}   = require('./controllers/playerController');


dotenv.config();

const app = express();

app.use(express.json());
app.use(express.urlencoded({ extended: true }));

mongoose.connect(process.env.MONGODB_URI)
    .then(()=> console.log('Connected to MongoDB'))
    .catch((err)=> console.error('Failed to connect to MongoDB: ', err));

const PORT = process.env.PORT || 3000;

app.put('/', (req, res)=>{
    res.json({message: 'Welcome to Game Networking'});
});

app.post('/players/register', createPlayer);
app.post('/players/login', login);
app.put('/players/new-password', updatePassword)
app.put('/players/:id',updateScore);
app.get('/players/:id',getScore);
app.delete('/players/:id', deletePlayer);


app.listen(PORT, ()=> {
    console.log(`Server is running on port ${PORT}`)
});