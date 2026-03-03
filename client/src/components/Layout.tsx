import { Outlet, NavLink } from 'react-router-dom';
import ProfileDropdown from './ProfileDropdown';
import GuidedTour from './GuidedTour';

function Layout() {
  return (
    <div className="layout">
      <GuidedTour />
      <nav className="navbar">
        <div className="container navbar-content">
          <NavLink to="/" className="navbar-brand">
            UniStart
          </NavLink>

          <ul className="navbar-nav">
            <li>
              <NavLink to="/" end>
                Главная
              </NavLink>
            </li>
            <li>
              <NavLink to="/learn">
                Обучение
              </NavLink>
            </li>
            <li>
              <NavLink to="/progress">
                Прогресс
              </NavLink>
            </li>
            <li>
              <NavLink to="/plan">
                План
              </NavLink>
            </li>
          </ul>

          <ProfileDropdown />
        </div>
      </nav>

      <main className="main-content">
        <div className="container">
          <Outlet />
        </div>
      </main>
    </div>
  );
}

export default Layout;
