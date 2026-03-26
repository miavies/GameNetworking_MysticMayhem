const mongoose = require('mongoose');

const playerSchema = new mongoose.Schema({
    username:{
        type: String,
        required:[true, 'Username is required'],
        unique: true,
        trim: true,
        minlength:[3, 'Username must be at least 3 characters'],
        maxlength:[20, 'Username cannot exceed 20 characters']    
    },

    email:{
        type: String,
        required:[true, 'Email is required'],
        unique: true,
        lowercase: true,
        match:[/^\S+@\S+.\S+$/,'Please provide a valid email']   
    },

    password:{
        type: String,
        required:[true, 'Password is required'],
        minlength: [8, 'Password must contain at least 8 characters'],
    },
    kills:{
        type: Number,
        default: 0,
        min:[0, 'Kills cannot be below 0']
    },
    deaths:{
        type: Number,
        default: 0,
        min:[0, 'Deaths cannot be below 0']
    },
 }, {
    timestamps: true
});

const Player = mongoose.model('Player', playerSchema);
module.exports = Player;