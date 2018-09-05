module.exports = {

  // source: tedious

  connectAndQuery: function(sql, callback) {  

    var express = require('express');
    var router = express.Router();

    var Connection = require('tedious').Connection;
    var config = require('../includes/config');
    var connection = new Connection(config);

    //var connection = requireonce('../includes/connect');
    var rows =[];

    // once connection is on --> execute statement
    connection.on('connect', function(err) {
      if (err) {
        console.log(err);
      } else {
        executeStatement(sql);
      }
    });

    // once connected - query can be executed

    var Request = require('tedious').Request;

    function executeStatement(sql) {
      // SQL STATEMENT HERE!!!!
      request = new Request(sql, function(err, rowCount) {
        if (err) {
          console.log(err);
        } else {
          console.log(rowCount + ' rows');
          console.log(sql);
        }
        connection.close();
      });

      // once there is a request --> make rows...
      request.on('row', function(columns) {
        var row = {};
        columns.forEach(function(column) {
          row[column.metadata.colName] = column.value;
        }); rows.push(row);
      });

      request.on('requestCompleted', function () {
      	return callback(rows);
      });

      connection.execSql(request);
    }
  }
}