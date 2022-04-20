
import TradeView from '../views/TradeView.jsx';
import SettingsView from '../views/SettingsView.jsx';

import {
  Apps,
  // Assessment,
  Home,
  Settings
} from '@material-ui/icons';

const appRoutes = [

  {
    path: "/trade",
    name: "Trade",
    icon: Home,
    component: TradeView,
  },
  // {
  //   path: "/test",
  //   name: "Test",
  //   icon: Assessment,
  //   component: TestView,
  // },
  {
    path: "/settings",
    name: "Settings",
    icon: Settings,
    component: SettingsView,
  },
];
export default appRoutes;
