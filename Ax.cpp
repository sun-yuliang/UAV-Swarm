#include<iostream>
#include<cmath>
using namespace std;
struct point{
	int x,y,z;
	point *next;
	point(){
		x=0;y=0;z=0;next=NULL;
	}
	point(int xx,int yy,int zz){
		x=xx;y=yy;z=zz;next=NULL;
	}
};
int s[102][102][102],g[102][102][102],d[102][102][102],f[102][102][102];
int dd[10][3]={{0,0,0},{1,0,0},{-1,0,0},{0,1,0},{0,-1,0},{0,0,1},{0,0,-1}};
int main(){
	point *l,*r,*t,*tt;
	int n,m,k,x1,x2,y1,y2,z1,z2,xx,yy,zz;
	bool p=false;
	
	cout<<"input the size of x,y,z:"<<endl;
	cin>>n>>m>>k;
	cout<<"input the map:"<<endl;
	for(int i=1;i<=n;i++)for(int j=1;j<=m;j++)for(int u=1;u<=k;u++)cin>>s[i][j][u];
	for(int i=1;i<=n;i++)for(int j=1;j<=m;j++){s[i][j][0]=1;s[i][j][k+1]=1;}
	for(int i=1;i<=n;i++)for(int u=1;u<=k;u++){s[i][0][u]=1;s[i][m+1][u]=1;}
	for(int j=1;j<=m;j++)for(int u=1;u<=k;u++){s[0][j][u]=1;s[n+1][j][u]=1;}
	
	cout<<"input the start point:";cin>>x1>>y1>>z1;
	cout<<"input the end point:";cin>>x2>>y2>>z2;
	l=new point(x1,y1,z1);
	r=l;
	while((l!=NULL)&&(p==false)){
		if((l->x==x2)&&(l->y==y2)&&(l->z==z2))p=true;
		if(p==false)if(s[l->x][l->y][l->z]==0)for(int i=d[l->x][l->y][l->z]+1;i<=d[l->x][l->y][l->z]+6;i++){  //本身方向最后计算 
			int ii=1+(i+5)%6;
			xx=l->x+dd[ii][0];yy=l->y+dd[ii][1];zz=l->z+dd[ii][2];
			if(s[xx][yy][zz]==0){
				if(g[xx][yy][zz]==0){
					g[xx][yy][zz]=g[l->x][l->y][l->z]+1;
					f[xx][yy][zz]=abs(x2-xx)+abs(y2-yy)+abs(z2-zz);
					d[xx][yy][zz]=ii;
					t=l;
					while((t->next!=NULL)&&(g[xx][yy][zz]+f[xx][yy][zz]>g[t->next->x][t->next->y][t->next->z]+f[t->next->x][t->next->y][t->next->z]))t=t->next;
					tt=new point(xx,yy,zz);
					tt->next=t->next;
					t->next=tt;
				}else if(g[xx][yy][zz]>g[l->x][l->y][l->z]+1){
					g[xx][yy][zz]=g[l->x][l->y][l->z]+1;
					d[xx][yy][zz]=ii;
					t=l;
					while((t->next!=NULL)&&(g[xx][yy][zz]+f[xx][yy][zz]>g[t->next->x][t->next->y][t->next->z]+f[t->next->x][t->next->y][t->next->z]))t=t->next;
					tt=new point(xx,yy,zz);
					tt->next=t->next;
					t->next=tt;
				}				
			}
		}
		s[l->x][l->y][l->z]=1;
		l=l->next;
	}
	if(p){
		r=new point(x2,y2,z2);
		l=new point;
		l->next=r;
		int ii;
		while(p){
			r=l->next;
			ii=d[r->x][r->y][r->z];
			xx=r->x-dd[ii][0];yy=r->y-dd[ii][1];zz=r->z-dd[ii][2];
			t=new point(xx,yy,zz);
			t->next=r;
			l->next=t;
			if((t->x==x1)&&(t->y==y1)&&(t->z==z1))p=false;
		}
		while(l->next!=NULL){
			l=l->next;
			cout<<"("<<l->x<<","<<l->y<<","<<l->z<<")";
			if(l->next!=NULL)cout<<"-->";
		}
	}else{
		cout<<"cannot find the way!";
	}
	return 0;
} 















