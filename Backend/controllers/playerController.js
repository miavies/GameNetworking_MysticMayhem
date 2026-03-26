const Player = require('../models/Player');

exports.createPlayer = async (req, res)=> {
    try{
        const player = await Player.create(req.body);

        res.status(201).json({
            success: true,
            message: 'Player created successfully',
            data: player
        }); 
    } 
    catch(error){

        if(error.name === 'ValidationError'){
            const messages = Object.values(error.errors).map(err => err.message);
            
            return res.status(400).json({
                success: false,
                message: 'Validation Error',
                error: messages
            });
        }

        if(error.code === 11000){
            const field = Object.keys(error.keyPattern)[0];

            return res.status(400).json({
                success: false,
                message: `${field} already exists`,
            });
        }

        res.status(500).json({
            success: false,
            message: 'Server Error',
            error: error.message
        })
    }
}

exports.login = async (req, res) => {
    try{
        const{username, password} = req.body;
        if(!username || !password){
            return res.status(400).json({
                success: false,
                message: "Please provide a valid username or password"
            });
        }

        const player = await Player.findOne ({username});
        if(!player || player.password !== password){
            return res.status(400).json({
                success: false,
                message: "Invalid username or password"
            });
        }

        res.status(200).json({
            success: true,
            message: "Login successful",
            data: player
        });
    }
    catch(error){
        res.status(500).json({
            success: false,
            error: error.message
        });
    }
}

exports.updateScore = async (req, res) =>{
    try{
        const{kills, deaths} = req.body;
        const id = req.params.id; // /player/:id
        const player = await Player.findById(id);

        if(!player){
            return res.status(404).json({
                success: false,
                message: 'Player not found'
            });
        }

        player.kills = player.kills + kills;
        player.deaths = player.deaths + deaths;

        await player.save();
        res.status(200).json({
            success: true,
            data:player
        });
    }
    catch(error){
        res.status(500).json({
            success: false,
            message: 'Failed to Update Player K/D',
            error: error.message
        });
    }
}

exports.getScore = async (req, res) =>{
    try{
        const id = req.params.id; // /player/:id
        const player = await Player.findById(id);

        if(!player){
            return res.status(404).json({
                success: false,
                message: 'Cannot find score'
            });
        }

        await player.save();
        res.status(200).json({
            success: true,
            data:player
        });
    }
    catch(error){
        res.status(500).json({
            success: false,
            message: 'Failed to get Player Score',
            error: error.message
        });
    }
}

exports.deletePlayer = async (req, res) => {
    try {
        const id = req.params.id; // /player/:id
        const player = await Player.findByIdAndDelete(id);

        if (!player) {
            return res.status(404).json({
                success: false,
                message: 'Player not found'
            });
        }

        res.status(200).json({
            success: true,
            message: 'Player account deleted successfully'
        });
    } catch (error) {
        res.status(500).json({
            success: false,
            message: 'Failed to delete player',
            error: error.message
        });
    }
};

exports.updatePassword = async (req, res) =>{
    try{
        const{username, newPassword, confirmPassword} = req.body;
        const player = await Player.findOne({username});

        if(!player || player.username !== username){
            return res.status(404).json({
                success: false,
                message: 'Player not found'
            });
        }

        if (newPassword === player.password){
            return res.status(400).json({
                success: false,
                message: 'New password cannot be the same as the old password'
            });
        }

        if (newPassword !== confirmPassword) {
            return res.status(400).json({
                success: false,
                message: 'Passwords do not match'
            });
        }

        player.password = newPassword;
        
        await player.save();
        res.status(200).json({
            success: true,
            message: 'Password updated successfully',
            data:player
        });
    }
    catch(error){
        res.status(500).json({
            success: false,
            message: 'Failed to update password',
            error: error.message
        });
    }
};