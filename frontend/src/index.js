import React from "react";
import ReactDOM from "react-dom";
import { createBrowserHistory } from "history";
import { HashRouter, Route, Switch } from "react-router-dom";
// import indexRoutes from "./routes/index.jsx";
import "./index.css";
import App from "./containers/App";

const indexRoutes = [{ path: "/", component: App }];

const hist = createBrowserHistory();

ReactDOM.render(
  <HashRouter history={hist}>

    <Switch>
      {indexRoutes.map((prop, key) => {
        //check if the user has logged in 
        return (
          <Route 
            //path='/app/' 
            path={prop.path} 
            component={prop.component} 
            key={key} 
          />
        );
      })}
    </Switch>
  </HashRouter>,
  document.getElementById("root")
);

