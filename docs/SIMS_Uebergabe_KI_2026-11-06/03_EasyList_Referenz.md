# Altes ProjektWeb / EasyList: Authentifizierungs-Referenz

Repo aus damaliger VS-Code-Ausgabe: `https://github.com/c2310475117/ProjektWeb_EasyList.git`; `be` (Backend), `fe` (Frontend), SQLite DB `be/Database/EasyListDB.sqlite`. **Altprojekt ist Node.js/Express/Sequelize, NICHT C#.** Es dient als mentales Vorbild, nicht als direkt kopierbare Implementierung.

## `fe/index.html` (relevante Ausschnitte aus Chat)
Login-Formular mit `YourName`, `Password`, Registrierungsformular mit `YourName`, `YourEmail`, `Password`; JS sendet:
```javascript
fetch('http://localhost:3000/user/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ username, password })
});
fetch('http://localhost:3000/user/register', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ username, email, password })
});
```
Bei Erfolg speicherte das Frontend `token` und `userId` in `localStorage` und leitete zu `todo.html?token=...&user_id=...` weiter. **Unsicheres Muster, nicht für SIMS kopieren.**

## `fe/index.js` (aus Nutzerbeitrag)
Nach Login `localStorage.getItem('token')`, `.getItem('userId')`, API-Anfragen `Authorization: Bearer <token>`; `GET /user/lists/{userId}` und `GET /user/{userId}`. Lädt/verwaltet Listen/Items/Medikamente, für SIMS fachlich irrelevant. `innerHTML` mit dynamischen `list.list_name` und `item.item_title_en` → potentielles XSS. In `deleteItem()` wird `listId` referenziert, ohne in diesem Scope definiert zu sein.

## `be/models/userModel.js` (inhaltlich vollständig)
```javascript
import { Model, DataTypes } from 'sequelize';
import Sequ from '../db.js';
class User extends Model {}
User.init({
  user_id: { type: DataTypes.INTEGER, autoIncrement: true, primaryKey: true },
  user_name: { type: DataTypes.STRING, allowNull: false, unique: true },
  password_hash: { type: DataTypes.STRING, allowNull: false },
  email: { type: DataTypes.STRING, allowNull: false, unique: true },
  created_at: { type: DataTypes.DATE, defaultValue: DataTypes.NOW },
  updated_at: { type: DataTypes.DATE, defaultValue: DataTypes.NOW }
}, {
  sequelize: Sequ, modelName: 'User', tableName: 'users',
  timestamps: true, underscored: true
});
export default User;
```

## `be/routes/userRoutes.js`: Registrieren und Login (gekürzter Original-Ablauf)
```javascript
// Registrierung: im Original POST /register (im Server vermutlich /user gemountet)
const { username, email, password } = req.body;
if (!username || !email || !password) return res.status(400).json({ message: '...' });
const existingUser = await User.findOne({
  where: { [Op.or]: [{ user_name: username }, { email: email }] }
});
if (existingUser) return res.status(409).json({ message: 'User already exists' });
const hashedPassword = await bcrypt.hash(password, 10);
const user = await User.create({
  user_name: username, email, password_hash: hashedPassword
});
// In EasyList wurde zusätzlich eine Standard-Liste erstellt.
const token = generateAccessToken(user.user_id);
res.status(201).json({ token, l_user_id: user.user_id, defaultList });

// Login: im Original POST /login
const { username, password } = req.body;
const user = await User.findOne({ where: { user_name: username } });
if (!user || !(await bcrypt.compare(password, user.password_hash))) {
  return res.status(401).json({ message: 'Invalid credentials' });
}
const token = generateAccessToken(user.user_id);
res.status(200).json({ token, user_id: user.user_id });
```
`userRoutes.js` enthielt außerdem geschützte Endpunkte für eigene Listen, Deletion usw. Die wichtige Logik ist: Username in SQL suchen → bcrypt-Hash prüfen → Token generieren → zurückgeben.

## `be/auth.js` (sinngemäß originalgetreu)
```javascript
import jwt from 'jsonwebtoken';
const secretKey = process.env.JWT_SECRET || 'geheimnis';
const generateAccessToken = (userId) =>
  jwt.sign({ userId }, secretKey, { expiresIn: '1h' });
const verifyToken = (token) => jwt.verify(token, secretKey);
const authMiddleware = (req, res, next) => {
  const authHeader = req.headers['authorization'];
  const token = authHeader && authHeader.split(' ')[1];
  if (!token) return res.status(401).json({ message: 'Authorization header missing' });
  try {
    req.user = verifyToken(token);
    next();
  } catch (error) {
    return res.status(403).json({ message: 'Unauthorized access' });
  }
};
```
Daneben gab es `checkListAccess` (Suchabfrage nach `list_name` UND authentifizierter `userId`) zur Objektzugriffskontrolle.

## Konkrete Qualitäts-/Sicherheitshinweise zur Altimplementierung
- Die Zeile `console.log('Request body:', req.body)` in Registrierung würde Passwort im Klartext loggen. **Nicht übernehmen!**
- JWT-Key-Fallback `'geheimnis'` ist unsicher. Neue Anwendung darf ohne sicheres Secret nicht signieren/starten.
- Alte Registrierung: Response-Feld `l_user_id`, Frontend erwartete `user_id` (Inkonsistenz).
- Alte Account-Löschung verwendete `verifyToken` direkt als Middleware, obwohl Funktion in `auth.js` nur einen Token annimmt (wahrscheinlich Fehler).
- In der Löschroute wurde ein `userId` aus Token mit URL-Parameter verglichen (unterschiedliche Typen möglich).
- Token in URL und `localStorage`: Leakage/XSS-Risiken. Authserver und Ticket-Service müssen konsistent authentifizieren/autorisieren.
- Altprojekt kennt noch keine Supportgruppen. SIMS-User-Rolle/Supportlevel muss aus SQL geladen und durch API serverseitig validiert werden.

## Noch NICHT aus dem Altprojekt erhalten
`be/db.js` und `be/server.js` wurden zwar angefordert, aber noch nicht von Nutzerin geschickt. Deshalb keine gesicherten Angaben zum Sequelize-Datenbank-Setup oder Router-Mounting, außer den gezeigten API-Aufrufen/Modellen.
