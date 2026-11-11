// backend/routes/userRoutes.js

import express from 'express';
import User from '../models/userModel.js';
import List from '../models/listModel.js';
import bcrypt from 'bcrypt';
import { generateAccessToken, verifyToken, authMiddleware } from '../auth.js';
import { Op } from 'sequelize';

const router = express.Router();

// Funktion zur Erstellung einer Liste für einen Benutzer
const createList = async (listName, userId) => {
  try {
    const newList = await List.create({
      list_name: listName,
      l_user_id: userId,
    });
    return newList;
  } catch (error) {
    console.error('Error creating list:', error);
    throw error;
  }
};

router.post('/lists', authMiddleware, async (req, res) => {
  const { listName } = req.body;
  const userId = req.user.userId; // Benutzer-ID aus dem Token

  try {
    // Hier erfolgt die Erstellung der Liste in der Datenbank
    const newList = await createList(listName, userId);
    res.status(201).json(newList);
  } catch (error) {
    console.error('Fehler beim Erstellen der Liste:', error);
    res.status(500).json({ error: 'Interner Serverfehler beim Erstellen der Liste' });
  }
});

router.get('/lists/:userId', authMiddleware, async (req, res) => {
  const userId = req.user.userId;

  try {
    const userLists = await List.findAll({ where: { l_user_id: userId } });
    res.status(200).json({ lists: userLists });
  } catch (error) {
    console.error('Fehler beim Abrufen der Listen des Benutzers:', error);
    res.status(500).json({ error: 'Interner Serverfehler beim Abrufen der Listen' });
  }
});

// Route zum Löschen einer Liste
router.delete('/lists/:listId', authMiddleware, async (req, res) => {
  const listId = req.params.listId;
  const userId = req.user.userId; // Benutzer-ID aus dem Token

  try {
    const list = await List.findOne({ where: { list_id: listId, l_user_id: userId } });

    if (!list) {
      return res.status(404).json({ message: 'Liste nicht gefunden' });
    }

    await list.destroy();
    res.status(204).json({ message: 'Liste erfolgreich gelöscht' });
  } catch (error) {
    console.error('Fehler beim Löschen der Liste:', error);
    res.status(500).json({ error: 'Interner Serverfehler beim Löschen der Liste' });
  }
});

// Registrierung eines neuen Benutzers
router.post('/register', async (req, res) => {
  const { username, email, password } = req.body;
  console.log('Request body:', req.body);

  if (!username || !email || !password) {
    console.log('Missing fields during registration');
    return res.status(400).json({ message: 'Name, Email, and Password are required' });
  }

  try {
    const existingUser = await User.findOne({ where: { [Op.or]: [{ user_name: username }, { email: email }] } });
    if (existingUser) {
      return res.status(409).json({ message: 'User already exists' });
    }

    const hashedPassword = await bcrypt.hash(password, 10);
    console.log('Creating user:', { username, email });

    const user = await User.create({ user_name: username, email, password_hash: hashedPassword });

    console.log('User created successfully:', user);

    const listName = "default List";
    const defaultList = await createList(listName, user.user_id);
    console.log('Default list created for user:', defaultList);

    const token = generateAccessToken(user.user_id);

    res.status(201).json({ token, l_user_id: user.user_id, defaultList }); // Geändert: Senden von JSON-Antworten statt Redirect
  } catch (error) {
    console.error('Error during registration:', error);
    res.status(500).json({ message: 'Internal server error', error: error.errors ? error.errors[0].message : error.message });
  }
});

// Anmeldung eines Benutzers
router.post('/login', async (req, res) => {
  const { username, password } = req.body;

  if (!username || !password) {
    return res.status(400).json({ message: 'Username and Password are required' });
  }

  try {
    const user = await User.findOne({ where: { user_name: username } });

    if (!user || !(await bcrypt.compare(password, user.password_hash))) {
      return res.status(401).json({ message: 'Invalid credentials' });
    }

    const token = generateAccessToken(user.user_id);
    res.status(200).json({ token, user_id: user.user_id }); // Geändert: Senden von JSON-Antworten statt Redirect
  } catch (error) {
    console.error('Error during login:', error);
    res.status(500).json({ message: 'Internal server error', error: error.message });
  }
});

// Benutzer löschen
router.delete('/:userId', verifyToken, authMiddleware, async (req, res) => {
  const userIdToDelete = req.params.userId; // User ID to delete
  const requestingUserId = req.user.userId; // User ID of requesting user

  try {
    if (requestingUserId !== userIdToDelete) {
      return res.status(403).json({ message: 'Keine Berechtigung, um den Benutzer zu löschen' });
    }

    const deletedUser = await User.findByPk(userIdToDelete);

    if (!deletedUser) {
      return res.status(404).json({ message: 'Benutzer nicht gefunden' });
    }

    await List.destroy({ where: { l_user_id: userIdToDelete } });
    await deletedUser.destroy();

    res.status(200).json({ message: 'Benutzer erfolgreich gelöscht' });
  } catch (error) {
    console.error('Fehler beim Löschen des Benutzers:', error);
    res.status(500).json({ error: 'Interner Serverfehler beim Löschen des Benutzers' });
  }
});

export default router;
