1. You might need `sudo service postgresql start` to start postgresql service
2. Use `sudo service postgresql status` to check status and `sudo service postgresql stop` to stop.
3. Use `sudo -u postgres psql` to connect to postgreSQL session.
4. In postfreSQL, use `\i <absolute_path_to_file>` to execute `sql/init.sql`.