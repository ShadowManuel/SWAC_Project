class ToDoListManager {
  constructor() {
    this.init();
  }

  async init() {
    const token = localStorage.getItem('token');
    const userId = localStorage.getItem('userId');

    if (!token || !userId) {
      console.error('User not authenticated.');
      return;
    }

    try {
      await this.fetchUserLists(userId);
      this.setupEventListeners();
      this.displayUserName();
    } catch (error) {
      console.error('Initialization error:', error);
    }
  }

  async fetchUserLists(userId) {
    try {
      const response = await fetch(`http://localhost:3000/user/lists/${userId}`, {
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`,
          'Content-Type': 'application/json'
        }
      });

      if (!response.ok) {
        throw new Error('Network response was not ok');
      }

      const data = await response.json();
      this.renderUserLists(data.lists);

      // Fetch username if not already in localStorage
      if (!localStorage.getItem('username')) {
        const userResponse = await fetch(`http://localhost:3000/user/${userId}`, {
          headers: {
            'Authorization': `Bearer ${localStorage.getItem('token')}`,
          }
        });
        const userData = await userResponse.json();
        if (userData.username) {
          localStorage.setItem('username', userData.username);
          this.displayUserName();
        } else {
          console.error('Username not found in user data');
        }
      } else {
        this.displayUserName();
      }
    } catch (error) {
      console.error('Error fetching user lists:', error);
    }
  }

  renderUserLists(lists) {
    const listContainer = document.getElementById('listContainer');
    listContainer.innerHTML = '';

    lists.forEach(list => {
      const listElement = document.createElement('div');
      listElement.classList.add('user-list');
      listElement.dataset.listId = list.list_id;
      listElement.innerHTML = `
        <div class="list-header">
          <h3>${list.list_name}</h3>
          <button class="delete-list-button">X</button>
        </div>
        <ul class="item-list"></ul>
        <form class="add-item-form">
          <input type="text" class="item-input" placeholder="Add new item or med...">
          <select class="item-type">
            <option value="item">Item</option>
            <option value="med">Med</option>
          </select>
          <button type="submit" class="add-item-button">Add</button>
        </form>
      `;

      const itemList = listElement.querySelector('.item-list');
      this.fetchItemsAndMeds(list.list_id, itemList);

      listContainer.appendChild(listElement);
    });
  }

  async fetchItemsAndMeds(listId, itemList) {
    try {
      const itemsResponse = await fetch(`http://localhost:3000/items?list_id=${listId}`, {
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`,
          'Content-Type': 'application/json'
        }
      });

      const medsResponse = await fetch(`http://localhost:3000/meds?list_id=${listId}`, {
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`,
          'Content-Type': 'application/json'
        }
      });

      if (!itemsResponse.ok || !medsResponse.ok) {
        throw new Error('Network response was not ok');
      }

      const itemsData = await itemsResponse.json();
      const medsData = await medsResponse.json();

      itemsData.forEach(item => {
        const itemElement = this.createItemElement(item, 'item');
        itemList.appendChild(itemElement);
      });

      medsData.forEach(med => {
        const medElement = this.createItemElement(med, 'med');
        itemList.appendChild(medElement);
      });
    } catch (error) {
      console.error('Error fetching items and meds:', error);
    }
  }

  createItemElement(item, type) {
    const itemElement = document.createElement('li');
    itemElement.className = 'todo-list-item';
    itemElement.dataset.itemId = item.id;
    itemElement.dataset.itemType = type;
    itemElement.innerHTML = `
      <div class="item-content">
        <input type="checkbox" class="item-checkbox">
        <div class="title">${item.item_title_en || item.med_title_en}</div>
        <button class="delete-item-button">X</button>
      </div>
    `;
    return itemElement;
  }

  async createList(listName) {
    try {
      const response = await fetch('http://localhost:3000/user/lists', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ listName })
      });

      if (!response.ok) {
        throw new Error('Network response was not ok');
      }

      await this.fetchUserLists(localStorage.getItem('userId'));
    } catch (error) {
      console.error('Error creating list:', error);
    }
  }

  async addItemToList(listId, itemTitle, itemType) {
    const endpoint = itemType === 'med' ? 'meds' : 'items';
    try {
      const response = await fetch(`http://localhost:3000/user/lists/${listId}/${endpoint}`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ keyword: itemTitle, userId: localStorage.getItem('userId') })
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(`Error: ${errorData.message}`);
      }

      await this.fetchUserLists(localStorage.getItem('userId'));
    } catch (error) {
      console.error('Error adding item to list:', error);
    }
  }

  async deleteList(listId) {
    try {
      const response = await fetch(`http://localhost:3000/user/lists/${listId}`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`,
          'Content-Type': 'application/json'
        }
      });

      if (!response.ok) {
        throw new Error('Network response was not ok');
      }

      await this.fetchUserLists(localStorage.getItem('userId'));
    } catch (error) {
      console.error('Error deleting list:', error);
    }
  }

  async deleteItem(itemId, itemType) {
    const endpoint = itemType === 'med' ? 'meds' : 'items';
    try {
      const response = await fetch(`http://localhost:3000/user/lists/${listId}/${endpoint}/${itemId}`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`,
          'Content-Type': 'application/json'
        }
      });

      if (!response.ok) {
        throw new Error('Network response was not ok');
      }

      await this.fetchUserLists(localStorage.getItem('userId'));
    } catch (error) {
      console.error('Error deleting item:', error);
    }
  }

  displayUserName() {
    const userName = localStorage.getItem('username');
    const welcomeMessage = document.getElementById('welcomeMessage');
    if (userName && welcomeMessage) {
      welcomeMessage.textContent = `Welcome to Easy List, ${userName}`;
    }
  }

  setupEventListeners() {
    document.getElementById('createListForm').addEventListener('submit', async (event) => {
      event.preventDefault();
      const listNameInput = document.getElementById('listName');
      const listName = listNameInput.value.trim();
      if (listName !== '') {
        await this.createList(listName);
        listNameInput.value = '';
      }
    });

    document.getElementById('listContainer').addEventListener('click', async (event) => {
      const target = event.target;

      if (target.classList.contains('add-item-button')) {
        const listElement = target.closest('.user-list');
        const listId = listElement.dataset.listId;
        const itemInput = listElement.querySelector('.item-input');
        const itemTitle = itemInput.value.trim();
        const itemType = listElement.querySelector('.item-type').value;
        if (itemTitle !== '') {
          await this.addItemToList(listId, itemTitle, itemType);
          itemInput.value = '';
        }
      }

      if (target.classList.contains('delete-list-button')) {
        const listElement = target.closest('.user-list');
        const listId = listElement.dataset.listId;
        await this.deleteList(listId);
      }

      if (target.classList.contains('delete-item-button')) {
        const itemElement = target.closest('.todo-list-item');
        const itemId = itemElement.dataset.itemId;
        const itemType = itemElement.dataset.itemType;
        await this.deleteItem(itemId, itemType);
      }
    });
  }
}

document.addEventListener('DOMContentLoaded', () => {
  new ToDoListManager();
});
