// App.tsx
import { Outlet } from 'react-router-dom';
import StaggeredMenu from './components/StaggeredMenu';


export default function App() {
  const menuItems = [
    { label: '首页', ariaLabel: '前往首页', link: '/' },
    { label: '产品', ariaLabel: '查看我们的产品', link: '/products' },
    { label: '关于', ariaLabel: '了解关于我们', link: '/about' },
    { label: '联系我们', ariaLabel: '联系我们', link: '/contact' }
  ];

  const socialItems = [
    { label: 'GitHub', link: 'https://github.com' }
  ];

  return (
    <>
      <StaggeredMenu
        position="left"
        items={menuItems}
        socialItems={socialItems}
        displaySocials={false}
        displayItemNumbering={true}
        menuButtonColor="#fff"
        openMenuButtonColor="#777"
        changeMenuColorOnOpen={true}
        colors={['#646CFF', '#FFD62E']}
        accentColor="#404041"
        logoUrl="/vite.svg"
        isFixed={true}
        onMenuOpen={() => console.log('Menu opened')}
        onMenuClose={() => console.log('Menu closed')}
      />

      <main style={{ padding: '2rem', minHeight: '60vh' }}>
        <Outlet />
      </main>

      <footer style={{ padding: '2rem', background: '#333', color: '#fff' }}>
        Footer
      </footer>
    </>
  );
}
