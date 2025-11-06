# Инструкция по публикации на GitHub

## Шаги для публикации проекта на GitHub:

### 1. Создайте репозиторий на GitHub

1. Перейдите на https://github.com
2. Нажмите кнопку **"New"** (или "+" в правом верхнем углу → "New repository")
3. Заполните данные:
   - **Repository name:** `network-monitoring-system-mau` (или любое другое имя)
   - **Description:** "Система мониторинга сетевого оборудования для МАУ"
   - **Visibility:** Public или Private (на ваш выбор)
   - **НЕ** создавайте README, .gitignore или лицензию (они уже есть в проекте)
4. Нажмите **"Create repository"**

### 2. Подключите локальный репозиторий к GitHub

После создания репозитория GitHub покажет инструкции. Выполните следующие команды:

```bash
# Добавьте удаленный репозиторий (замените YOUR_USERNAME на ваш GitHub username)
git remote add origin https://github.com/YOUR_USERNAME/network-monitoring-system-mau.git

# Отправьте код на GitHub
git push -u origin main
```

### 3. Альтернативный способ (если репозиторий уже создан)

Если репозиторий уже существует, используйте:

```bash
# Удалите существующий remote (если был)
git remote remove origin

# Добавьте новый remote
git remote add origin https://github.com/YOUR_USERNAME/REPOSITORY_NAME.git

# Отправьте код
git push -u origin main
```

### 4. Проверка

После выполнения команд проверьте:
- Откройте ваш репозиторий на GitHub
- Убедитесь, что все файлы загружены
- Проверьте, что README.md отображается корректно

## Дополнительные команды

### Добавить теги (для версий)

```bash
# Создать тег для версии 1.0.0
git tag -a v1.0.0 -m "Версия 1.0.0 - Стабильный релиз"
git push origin v1.0.0
```

### Обновление проекта

При внесении изменений:

```bash
# Проверить статус
git status

# Добавить изменения
git add .

# Создать коммит
git commit -m "Описание изменений"

# Отправить на GitHub
git push
```

## Полезные ссылки

- [GitHub Guides](https://guides.github.com/)
- [Git Documentation](https://git-scm.com/doc)
- [GitHub Desktop](https://desktop.github.com/) - графический клиент для работы с Git

---

**Примечание:** Если у вас возникнут проблемы с авторизацией, используйте Personal Access Token вместо пароля:
1. GitHub → Settings → Developer settings → Personal access tokens → Tokens (classic)
2. Создайте новый токен с правами `repo`
3. Используйте токен вместо пароля при push

